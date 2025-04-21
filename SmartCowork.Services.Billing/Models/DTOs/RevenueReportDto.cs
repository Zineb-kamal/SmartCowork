// DTOs/RevenueReportDto.cs
using System;
using System.Collections.Generic;

namespace SmartCowork.Services.Billing.DTOs
{
    /// <summary>
    /// DTO pour représenter un rapport de revenus sur une période définie
    /// </summary>
    public class RevenueReportDto
    {
        /// <summary>
        /// Date de début de la période du rapport
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Date de fin de la période du rapport
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Revenu total sur la période (tous paiements confirmés)
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Montant total des factures émises (payées ou non)
        /// </summary>
        public decimal TotalBilled { get; set; }

        /// <summary>
        /// Montant total des factures en attente de paiement
        /// </summary>
        public decimal TotalPending { get; set; }

        /// <summary>
        /// Montant total des factures annulées
        /// </summary>
        public decimal TotalCancelled { get; set; }

        /// <summary>
        /// Montant total des factures en retard de paiement
        /// </summary>
        public decimal TotalOverdue { get; set; }

        /// <summary>
        /// Nombre total de factures émises
        /// </summary>
        public int TotalInvoices { get; set; }

        /// <summary>
        /// Nombre de factures payées
        /// </summary>
        public int PaidInvoices { get; set; }

        /// <summary>
        /// Nombre de factures en attente de paiement
        /// </summary>
        public int PendingInvoices { get; set; }

        /// <summary>
        /// Nombre de factures annulées
        /// </summary>
        public int CancelledInvoices { get; set; }

        /// <summary>
        /// Nombre de factures en retard de paiement
        /// </summary>
        public int OverdueInvoices { get; set; }

        /// <summary>
        /// Taux de conversion (factures payées / factures émises)
        /// </summary>
        public decimal ConversionRate { get; set; }

        /// <summary>
        /// Nombre total de transactions de paiement
        /// </summary>
        public int TotalTransactions { get; set; }

        /// <summary>
        /// Montant moyen des transactions
        /// </summary>
        public decimal AverageTransactionAmount { get; set; }

        /// <summary>
        /// Revenu moyen par jour sur la période
        /// </summary>
        public decimal AverageDailyRevenue { get; set; }

        /// <summary>
        /// Répartition des revenus par méthode de paiement
        /// </summary>
        public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; } = new Dictionary<string, decimal>();

        /// <summary>
        /// Répartition des revenus par type d'espace
        /// </summary>
        public Dictionary<string, decimal> RevenueBySpaceType { get; set; } = new Dictionary<string, decimal>();

        /// <summary>
        /// Répartition des revenus par mois
        /// </summary>
        public Dictionary<string, decimal> RevenueByMonth { get; set; } = new Dictionary<string, decimal>();

        /// <summary>
        /// Top clients par revenu généré
        /// </summary>
        public List<TopClientDto> TopClients { get; set; } = new List<TopClientDto>();
    }

    /// <summary>
    /// DTO pour représenter un client dans le cadre d'un rapport de revenus
    /// </summary>
    public class TopClientDto
    {
        /// <summary>
        /// ID du client
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Nom du client (si disponible)
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Montant total dépensé
        /// </summary>
        public decimal TotalSpent { get; set; }

        /// <summary>
        /// Nombre de réservations effectuées
        /// </summary>
        public int BookingsCount { get; set; }
    }
}