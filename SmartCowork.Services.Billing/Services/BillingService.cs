// Services/BillingService.cs
using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartCowork.Common.Messaging.RabbitMQ;
using SmartCowork.Services.Billing.DTOs;
using SmartCowork.Services.Billing.Messages;
using SmartCowork.Services.Billing.Models;
using SmartCowork.Services.Billing.Models.DTOs;
using SmartCowork.Services.Billing.Repository;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SmartCowork.Services.Billing.Services
{
    public class BillingService : IBillingService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BillingService> _logger;
        private readonly IRabbitMQProducer _rabbitMQProducer;
        private readonly IUserService _userService;

        public BillingService(
            IInvoiceRepository invoiceRepository,
            IMapper mapper,
            ILogger<BillingService> logger,
            IRabbitMQProducer rabbitMQProducer,
            IUserService userService)
        {
            _invoiceRepository = invoiceRepository;
            _mapper = mapper;
            _logger = logger;
            _rabbitMQProducer = rabbitMQProducer;
            _userService = userService;
        }

        #region Méthodes CRUD Standard

        public async Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync()
        {
            var invoices = await _invoiceRepository.GetAllInvoicesAsync();
            return _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        }

        public async Task<IEnumerable<InvoiceDto>> GetUserInvoicesAsync(Guid userId)
        {
            var invoices = await _invoiceRepository.GetInvoicesByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        }

        public async Task<InvoiceDto> GetInvoiceByIdAsync(Guid id)
        {
            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(id);
            if (invoice == null)
                return null;

            return _mapper.Map<InvoiceDto>(invoice);
        }

        public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceDto createInvoiceDto)
        {
            var invoice = new Invoice
            {
                UserId = createInvoiceDto.UserId,
                DueDate = createInvoiceDto.DueDate ?? DateTime.UtcNow.AddDays(30),
                Status = InvoiceStatus.Pending,
                Items = createInvoiceDto.Items.Select(item => new InvoiceItem
                {
                    BookingId = item.BookingId,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice
                }).ToList()
            };

            var createdInvoice = await _invoiceRepository.CreateInvoiceAsync(invoice);

            // Publier un événement pour informer d'autres services
            try
            {
                var invoiceItemInfos = createdInvoice.Items.Select(item => new InvoiceItemInfo
                {
                    BookingId = item.BookingId,
                    Description = item.Description,
                    Amount = item.TotalPrice
                }).ToList();

                _rabbitMQProducer.PublishMessage(
                    "billing_events",
                    "invoice.created",
                    new InvoiceCreatedMessage
                    {
                        InvoiceId = createdInvoice.Id,
                        UserId = createdInvoice.UserId,
                        TotalAmount = createdInvoice.TotalAmount,
                        Status = createdInvoice.Status.ToString(),
                        DueDate = createdInvoice.DueDate,
                        Items = invoiceItemInfos,
                        CreatedAt = DateTime.UtcNow
                    });

                _logger.LogInformation($"Événement publié: facture {createdInvoice.Id} créée");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Impossible de publier l'événement de création de facture: {ex.Message}");
                // Continue malgré l'erreur
            }

            return _mapper.Map<InvoiceDto>(createdInvoice);
        }

        public async Task<InvoiceDto> UpdateInvoiceAsync(UpdateInvoiceDto updateInvoiceDto)
        {
            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(updateInvoiceDto.Id);
            if (invoice == null)
                return null;

            if (updateInvoiceDto.DueDate.HasValue)
                invoice.DueDate = updateInvoiceDto.DueDate.Value;

            if (updateInvoiceDto.Status.HasValue)
                invoice.Status = updateInvoiceDto.Status.Value;

            await _invoiceRepository.UpdateInvoiceAsync(invoice);
            return _mapper.Map<InvoiceDto>(invoice);
        }

        public async Task<bool> DeleteInvoiceAsync(Guid id)
        {
            return await _invoiceRepository.DeleteInvoiceAsync(id);
        }

        public async Task<IEnumerable<TransactionDto>> GetInvoiceTransactionsAsync(Guid invoiceId)
        {
            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
            if (invoice == null)
                return new List<TransactionDto>();

            return _mapper.Map<IEnumerable<TransactionDto>>(invoice.Transactions);
        }

        public async Task<IEnumerable<InvoiceDto>> GetOverdueInvoicesAsync()
        {
            var overdueInvoices = await _invoiceRepository.GetOverdueInvoicesAsync();
            return _mapper.Map<IEnumerable<InvoiceDto>>(overdueInvoices);
        }

        #endregion

        #region Méthodes de paiement avec publication d'événements

        public async Task<TransactionDto> ProcessPaymentAsync(CreateTransactionDto createTransactionDto)
        {
            var transaction = new Transaction
            {
                InvoiceId = createTransactionDto.InvoiceId,
                Amount = createTransactionDto.Amount,
                PaymentMethod = createTransactionDto.PaymentMethod,
                ReferenceNumber = createTransactionDto.ReferenceNumber,
                Status = TransactionStatus.Completed,
                Date = DateTime.UtcNow
            };

            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(createTransactionDto.InvoiceId);
            if (invoice == null)
                throw new KeyNotFoundException($"Facture {createTransactionDto.InvoiceId} non trouvée");

            // Ajouter la transaction
            invoice.Transactions.Add(transaction);

            // Calculer le total des paiements
            var totalPaid = invoice.Transactions
                .Where(t => t.Status == TransactionStatus.Completed)
                .Sum(t => t.Amount) + transaction.Amount;

            // Mettre à jour le statut de la facture si le paiement est complet
            var oldStatus = invoice.Status;
            if (totalPaid >= invoice.TotalAmount)
            {
                invoice.Status = InvoiceStatus.Paid;
            }

            // Sauvegarder les modifications
            await _invoiceRepository.UpdateInvoiceAsync(invoice);

            // Identifier les réservations concernées par cette facture
            var bookingIds = invoice.Items.Select(item => item.BookingId).ToList();

            // Publier un événement de paiement traité
            try
            {
                foreach (var bookingId in bookingIds)
                {
                    _rabbitMQProducer.PublishMessage(
                        "billing_events",
                        "payment.processed",
                        new PaymentProcessedMessage
                        {
                            PaymentId = transaction.Id,
                            InvoiceId = invoice.Id,
                            BookingId = bookingId,
                            UserId = invoice.UserId,
                            Amount = transaction.Amount,
                            PaymentMethod = transaction.PaymentMethod.ToString(),
                            Status = transaction.Status.ToString(),
                            ReferenceNumber = transaction.ReferenceNumber,
                            ProcessedAt = transaction.Date
                        });
                }

                _logger.LogInformation($"Événement(s) de paiement publié(s) pour la facture {invoice.Id}");

                // Si la facture est maintenant payée, publier un événement supplémentaire
                if (oldStatus != InvoiceStatus.Paid && invoice.Status == InvoiceStatus.Paid)
                {
                    _rabbitMQProducer.PublishMessage(
                        "billing_events",
                        "invoice.paid",
                        new InvoicePaidMessage
                        {
                            InvoiceId = invoice.Id,
                            UserId = invoice.UserId,
                            TotalAmount = invoice.TotalAmount,
                            PaidAt = DateTime.UtcNow
                        });

                    _logger.LogInformation($"Événement publié: facture {invoice.Id} payée");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Impossible de publier l'événement de paiement: {ex.Message}");
                // Continue malgré l'erreur
            }

            return _mapper.Map<TransactionDto>(transaction);
        }

        public async Task<TransactionDto> ProcessRefundAsync(Guid transactionId, decimal amount, string reason)
        {
            // Trouver la transaction originale
            var invoice = await _invoiceRepository.FindInvoiceByTransactionIdAsync(transactionId);
            if (invoice == null)
                throw new KeyNotFoundException($"Transaction {transactionId} non trouvée");

            var originalTransaction = invoice.Transactions.FirstOrDefault(t => t.Id == transactionId);
            if (originalTransaction == null)
                throw new KeyNotFoundException($"Transaction {transactionId} non trouvée");

            // Créer une transaction de remboursement
            var refundTransaction = new Transaction
            {
                InvoiceId = invoice.Id,
                Amount = -amount, // Montant négatif pour un remboursement
                PaymentMethod = originalTransaction.PaymentMethod,
                ReferenceNumber = $"REFUND-{originalTransaction.ReferenceNumber}",
                Status = TransactionStatus.Refunded,
                Date = DateTime.UtcNow
            };

            // Ajouter la transaction de remboursement
            invoice.Transactions.Add(refundTransaction);

            // Recalculer le total des paiements
            var totalPaid = invoice.Transactions
                .Where(t => t.Status == TransactionStatus.Completed || t.Status == TransactionStatus.Refunded)
                .Sum(t => t.Amount);

            // Mettre à jour le statut de la facture si nécessaire
            if (totalPaid < invoice.TotalAmount && invoice.Status == InvoiceStatus.Paid)
            {
                invoice.Status = InvoiceStatus.Pending;
            }

            // Sauvegarder les modifications
            await _invoiceRepository.UpdateInvoiceAsync(invoice);

            // Identifier les réservations concernées par cette facture
            var bookingIds = invoice.Items.Select(item => item.BookingId).ToList();

            // Publier un événement de remboursement
            try
            {
                foreach (var bookingId in bookingIds)
                {
                    _rabbitMQProducer.PublishMessage(
                        "billing_events",
                        "payment.refunded",
                        new PaymentProcessedMessage
                        {
                            PaymentId = refundTransaction.Id,
                            InvoiceId = invoice.Id,
                            BookingId = bookingId,
                            UserId = invoice.UserId,
                            Amount = -amount, // Montant négatif pour un remboursement
                            PaymentMethod = refundTransaction.PaymentMethod.ToString(),
                            Status = "Refunded",
                            ReferenceNumber = refundTransaction.ReferenceNumber,
                            ProcessedAt = refundTransaction.Date
                        });
                }

                _logger.LogInformation($"Événement(s) de remboursement publié(s) pour la facture {invoice.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Impossible de publier l'événement de remboursement: {ex.Message}");
                // Continue malgré l'erreur
            }

            return _mapper.Map<TransactionDto>(refundTransaction);
        }

        public async Task<RevenueReportDto> GenerateRevenueReportAsync(DateTime startDate, DateTime endDate)
        {
            // Logique pour générer un rapport de revenus
            var invoices = await _invoiceRepository.GetInvoicesByDateRangeAsync(startDate, endDate);

            var report = new RevenueReportDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = invoices.Sum(i => i.Transactions
                    .Where(t => t.Status == TransactionStatus.Completed)
                    .Sum(t => t.Amount)),
                TotalInvoices = invoices.Count(),
                PaidInvoices = invoices.Count(i => i.Status == InvoiceStatus.Paid),
                PendingInvoices = invoices.Count(i => i.Status == InvoiceStatus.Pending),
                CancelledInvoices = invoices.Count(i => i.Status == InvoiceStatus.Cancelled),
                OverdueInvoices = invoices.Count(i => i.Status == InvoiceStatus.Overdue)
            };

            return report;
        }

        #endregion

        #region Méthodes de traitement des messages RabbitMQ

        public async Task ProcessBookingCreatedAsync(BookingCreatedMessage message)
        {
            _logger.LogInformation($"Traitement du message de réservation créée: {message.BookingId}");

            try
            {
                // Calculer la durée en heures
                var duration = (message.EndDateTime - message.StartDateTime).TotalHours;
                var totalAmount = (decimal)duration * message.HourlyRate;

                // Créer une facture pour cette réservation
                var createInvoiceDto = new CreateInvoiceDto
                {
                    UserId = message.UserId,
                    DueDate = DateTime.UtcNow.AddDays(30), // Échéance standard à 30 jours
                    Items = new List<CreateInvoiceItemDto>
                    {
                        new CreateInvoiceItemDto
                        {
                            BookingId = message.BookingId,
                            Description = $"Réservation de {message.SpaceName ?? "l'espace"} du {message.StartDateTime:g} au {message.EndDateTime:g}",
                            Quantity = (decimal)duration,
                            UnitPrice = message.HourlyRate
                        }
                    }
                };

                await CreateInvoiceAsync(createInvoiceDto);
                _logger.LogInformation($"Facture créée pour la réservation {message.BookingId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la création de la facture pour la réservation {message.BookingId}: {ex.Message}");
                throw;
            }
        }

        public async Task ProcessBookingUpdatedAsync(BookingUpdatedMessage message)
        {
            _logger.LogInformation($"Traitement du message de réservation mise à jour: {message.BookingId}");

            try
            {
                // Trouver les factures liées à cette réservation
                var invoices = await _invoiceRepository.FindInvoicesByBookingIdAsync(message.BookingId);
                if (!invoices.Any())
                {
                    _logger.LogWarning($"Aucune facture trouvée pour la réservation {message.BookingId}");
                    return;
                }

                foreach (var invoice in invoices)
                {
                    // Ne mettre à jour que les factures en attente de paiement
                    if (invoice.Status != InvoiceStatus.Pending)
                    {
                        _logger.LogInformation($"La facture {invoice.Id} n'est pas en statut Pending, aucune mise à jour nécessaire");
                        continue;
                    }

                    // Trouver l'élément de facture correspondant à cette réservation
                    var invoiceItem = invoice.Items.FirstOrDefault(item => item.BookingId == message.BookingId);
                    if (invoiceItem == null)
                    {
                        _logger.LogWarning($"Élément de facture non trouvé pour la réservation {message.BookingId} dans la facture {invoice.Id}");
                        continue;
                    }

                    // Calculer la nouvelle durée en heures
                    var oldDuration = (decimal)(message.PreviousEndDateTime - message.PreviousStartDateTime).TotalHours;
                    var newDuration = (decimal)(message.NewEndDateTime - message.NewStartDateTime).TotalHours;

                    // Mettre à jour l'élément de facture
                    invoiceItem.Quantity = newDuration;
                    invoiceItem.TotalPrice = newDuration * invoiceItem.UnitPrice;
                    invoiceItem.Description = $"Réservation du {message.NewStartDateTime:g} au {message.NewEndDateTime:g}";

                    // Recalculer le montant total de la facture
                    invoice.TotalAmount = invoice.Items.Sum(item => item.TotalPrice);

                    // Sauvegarder les modifications
                    await _invoiceRepository.UpdateInvoiceAsync(invoice);
                    _logger.LogInformation($"Facture {invoice.Id} mise à jour suite à la modification de la réservation {message.BookingId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la mise à jour des factures pour la réservation {message.BookingId}: {ex.Message}");
                throw;
            }
        }

        public async Task ProcessBookingCancelledAsync(BookingMessages message)
        {
            _logger.LogInformation($"Traitement du message d'annulation de réservation: {message.BookingId}");

            try
            {
                // Trouver les factures liées à cette réservation
                var invoices = await _invoiceRepository.FindInvoicesByBookingIdAsync(message.BookingId);
                if (!invoices.Any())
                {
                    _logger.LogWarning($"Aucune facture trouvée pour la réservation {message.BookingId}");
                    return;
                }

                foreach (var invoice in invoices)
                {
                    if (invoice.Status == InvoiceStatus.Pending)
                    {
                        // Si la facture est en attente, nous pouvons l'annuler
                        invoice.Status = InvoiceStatus.Cancelled;
                        await _invoiceRepository.UpdateInvoiceAsync(invoice);

                        // Publier un événement d'annulation de facture
                        _rabbitMQProducer.PublishMessage(
                            "billing_events",
                            "invoice.cancelled",
                            new InvoiceCancelledMessage
                            {
                                InvoiceId = invoice.Id,
                                UserId = invoice.UserId,
                                Reason = $"Annulation de la réservation: {message.CancellationReason ?? "Raison non spécifiée"}",
                                CancelledAt = DateTime.UtcNow
                            });

                        _logger.LogInformation($"Facture {invoice.Id} annulée suite à l'annulation de la réservation {message.BookingId}");
                    }
                    else if (invoice.Status == InvoiceStatus.Paid)
                    {
                        // Si la facture est déjà payée, nous devons créer un remboursement
                        // Dans un système réel, cela pourrait déclencher un processus de remboursement manuel
                        // ou automatique selon votre logique métier

                        _logger.LogInformation($"La facture {invoice.Id} est déjà payée, un remboursement pourrait être nécessaire");

                        // Ici, vous pourriez ajouter une logique de remboursement automatique
                        // ou créer une tâche pour un traitement manuel
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors du traitement de l'annulation de réservation {message.BookingId}: {ex.Message}");
                throw;
            }
        }

        public async Task ProcessBookingCompletedAsync(BookingCompletedMessage message)
        {
            _logger.LogInformation($"Traitement du message de réservation terminée: {message.BookingId}");

            try
            {
                // Trouver les factures liées à cette réservation
                var invoices = await _invoiceRepository.FindInvoicesByBookingIdAsync(message.BookingId);
                if (!invoices.Any())
                {
                    _logger.LogWarning($"Aucune facture trouvée pour la réservation {message.BookingId}");
                    return;
                }

                foreach (var invoice in invoices)
                {
                    // Si la facture est toujours en attente, nous pourrions envoyer un rappel de paiement
                    if (invoice.Status == InvoiceStatus.Pending)
                    {
                        // Ici, vous pourriez déclencher l'envoi d'un rappel de paiement
                        _logger.LogInformation($"La réservation {message.BookingId} est terminée mais la facture {invoice.Id} est toujours en attente de paiement");

                        // Vous pourriez également mettre à jour une date de rappel ou un compteur de rappels
                    }
                }

                _logger.LogInformation($"Traitement terminé pour la réservation complétée {message.BookingId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors du traitement de la réservation terminée {message.BookingId}: {ex.Message}");
                throw;
            }
        }
        public async Task<byte[]> GenerateInvoicePdfAsync(Guid invoiceId)
        {
          
            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
            if (invoice == null)
                return null;

            _logger.LogInformation($"[PDF] Début de génération pour facture {invoiceId}");
            _logger.LogInformation($"[PDF] UserId: {invoice.UserId}");
            _logger.LogInformation($"[PDF] Nombre d'éléments: {invoice.Items?.Count ?? 0}");
            // Récupérer les données utilisateur
            var user = await _userService.GetUserByIdAsync(invoice.UserId);
            var userName = user?.FullName ?? "Client";

            // Enrichir les éléments de facture avec les données de réservation
            if (invoice.Items != null && invoice.Items.Any())
            {
                foreach (var item in invoice.Items)
                {
                    // Si le BookingId est présent, récupérer les détails de la réservation
                    if (item.BookingId != Guid.Empty)
                    {
                        //try
                        //{
                        //    var booking = await _bookingService.GetBookingByIdAsync(item.BookingId);
                        //    if (booking != null)
                        //    {
                        //        // Enrichir la description avec les vraies dates
                        //        item.Description = $"Réservation de {booking.SpaceName ?? "l'espace"} du " +
                        //                          $"{booking.StartDateTime:dd/MM/yyyy HH:mm} au " +
                        //                          $"{booking.EndDateTime:dd/MM/yyyy HH:mm}";

                        //        // Recalculer la quantité si nécessaire (en heures)
                        //        if (item.Quantity == 0)
                        //        {
                        //            var duration = (booking.EndDateTime - booking.StartDateTime).TotalHours;
                        //            item.Quantity = (decimal)duration;

                        //            // Si le prix unitaire est aussi à 0, essayer de le récupérer
                        //            if (item.UnitPrice == 0 && booking.HourlyRate.HasValue)
                        //            {
                        //                item.UnitPrice = booking.HourlyRate.Value;
                        //                item.TotalPrice = item.Quantity * item.UnitPrice;
                        //            }
                        //        }
                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    _logger.LogWarning(ex, $"Impossible de récupérer les détails de la réservation {item.BookingId}");
                        //    // Continuer malgré l'erreur
                        //}
                    }
                }
            }

            QuestPDF.Settings.License = LicenseType.Community;

            var pdfBytes = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    page.Header().Element(header =>
                    {
                        header.Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("SmartCowork").FontSize(20).Bold();
                                col.Item().Text("Espaces de travail intelligents").FontSize(12);
                            });

                            row.RelativeItem().AlignRight().Text($"Facture #{invoice.Id}").Bold().FontSize(20);
                        });
                    });

                    page.Content().Element(content =>
                    {
                        content.PaddingVertical(20).Column(column =>
                        {
                            // Informations client et facture
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn();
                                    cols.RelativeColumn();
                                });

                                table.Cell().Text("Informations client").Bold();
                                table.Cell().Text("Détails de la facture").Bold();

                                table.Cell().Column(col =>
                                {
                                    col.Item().Text($"Nom: {userName}");
                                    if (user != null)
                                    {
                                        col.Item().Text($"Email: {user.Email}");
                                        //if (!string.IsNullOrEmpty(user.PhoneNumber))
                                        //    col.Item().Text($"Téléphone: {user.PhoneNumber}");
                                    }
                                });

                                table.Cell().Column(col =>
                                {
                                    col.Item().Text($"Numéro: INV-{invoice.Id.ToString().Substring(0, 8).ToUpper()}");
                                    col.Item().Text($"Date d'émission: {invoice.CreatedDate:dd/MM/yyyy}");
                                    col.Item().Text($"Date d'échéance: {invoice.DueDate:dd/MM/yyyy}");
                                    col.Item().Text($"Statut: {invoice.Status}");
                                });
                            });

                            column.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            // Table des éléments de facture
                            column.Item().PaddingTop(20).Table(table =>
                            {
                                // Définir les colonnes
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);    // Description
                                    columns.RelativeColumn();     // Quantité
                                    columns.RelativeColumn();     // Prix unitaire
                                    columns.RelativeColumn();     // Total
                                });

                                // En-têtes
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Description").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Quantité").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Prix unitaire").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Total").Bold();
                                });

                                // Contenu des lignes
                                if (invoice.Items != null && invoice.Items.Any())
                                {
                                    foreach (var item in invoice.Items)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Description);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Quantity.ToString("0.##") + " h");
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{item.UnitPrice:0.##} €");
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{item.TotalPrice:0.##} €");
                                    }
                                }
                                else
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Aucun élément");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("-");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("-");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("-");
                                }
                            });

                            // Résumé de la facture
                            column.Item().PaddingTop(10).AlignRight().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn();
                                    cols.ConstantColumn(100);
                                });

                                table.Cell().Text("Total HT:").Bold();
                                table.Cell().Text($"{invoice.TotalAmount:0.##} €");

                                table.Cell().Text("TVA (20%):").Bold();
                                table.Cell().Text($"{invoice.TotalAmount * 0.2m:0.##} €");

                                table.Cell().Text("Total TTC:").Bold();
                                table.Cell().Text($"{invoice.TotalAmount * 1.2m:0.##} €").Bold();
                            });

                            // Instructions de paiement
                            column.Item().PaddingTop(20).Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
                            {
                                col.Item().Text("Instructions de paiement").Bold();
                                col.Item().Text("Veuillez effectuer le paiement avant la date d'échéance.");
                                col.Item().Text("Coordonnées bancaires: IBAN: FR76 XXXX XXXX XXXX XXXX XXXX XXX");
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("© 2025 SmartCowork - ");
                        x.Span("Page ").FontSize(10);
                        x.CurrentPageNumber().FontSize(10);
                        x.Span(" sur ").FontSize(10);
                        x.TotalPages().FontSize(10);
                    });
                });
            }).GeneratePdf();

            return pdfBytes;
        }
        #endregion
    }

    
}