using System;
using System.Collections.Generic;
using System.Text;

namespace ShipStation.Data
{
    public class ShipStationConstants
    {
        public const string APP_NAME = "ship-station";
        public const string APP_TOKEN = "X-Vtex-Api-AppToken";
        public const string APP_KEY = "X-Vtex-Api-AppKey";
        public const string ENDPOINT_KEY = "hook-notification";
        public const string FORWARDED_HEADER = "X-Forwarded-For";
        public const string FORWARDED_HOST = "X-Forwarded-Host";
        public const string APPLICATION_JSON = "application/json";
        public const string APPLICATION_FORM = "application/x-www-form-urlencoded";
        public const string HEADER_VTEX_CREDENTIAL = "X-Vtex-Credential";
        public const string AUTHORIZATION_HEADER_NAME = "Authorization";
        public const string PROXY_AUTHORIZATION_HEADER_NAME = "Proxy-Authorization";
        public const string USE_HTTPS_HEADER_NAME = "X-Vtex-Use-Https";
        public const string PROXY_TO_HEADER_NAME = "X-Vtex-Proxy-To";
        public const string VTEX_ACCOUNT_HEADER_NAME = "X-Vtex-Account";
        public const string ENVIRONMENT = "vtexcommercestable";
        public const string LOCAL_ENVIRONMENT = "myvtex";
        public const string VTEX_ID_HEADER_NAME = "VtexIdclientAutCookie";
        public const string VTEX_WORKSPACE_HEADER_NAME = "X-Vtex-Workspace";
        public const string APP_SETTINGS = "vtex.ship-station";
        public const string ACCEPT = "Accept";
        public const string CONTENT_TYPE = "Content-Type";
        public const string HTTP_FORWARDED_HEADER = "HTTP_X_FORWARDED_FOR";
        public const string BUCKET = "ship-station";
        public const string HOOK_PING = "ping";
        public const string WEB_HOOK_NOTIFICATION = "web-hook-notification";
        public const string STORE_LIST = "store-list";
        public const string SHIPMENT_CHECK = "shipment-check";
        public const string CANCELLED_ORDER_CHECK = "cancelled-order-check";

        public const string ORDER_CHANGE_REASON = "Cancelled by ShipStation";

        public class API
        {
            public const string HOST = "ssapi.shipstation.com";
            public const string ORDERS = "orders";
            public const string CREATE_ORDER = "createorder";
            public const string WEBHOOKS = "webhooks";
            public const string SUBSCRIBE = "subscribe";
            public const string SHIPMENTS = "shipments";
            public const string FULFILLMENTS = "fulfillments";
            public const string WAREHOUSES = "warehouses";
            public const string CREATE_WAREHOUSE = "createwarehouse";
            public const string STORES = "stores";
        }

        public class VtexOrderStatus
        {
            public const string OrderCreated = "order-created";
            public const string OrderCompleted = "order-completed";
            public const string OnOrderCompleted = "on-order-completed";
            public const string PaymentPending = "payment-pending";
            public const string WaitingForOrderAuthorization = "waiting-for-order-authorization";
            public const string ApprovePayment = "approve-payment";
            public const string PaymentApproved = "payment-approved";
            public const string PaymentDenied = "payment-denied";
            public const string RequestCancel = "request-cancel";
            public const string WaitingForSellerDecision = "waiting-for-seller-decision";
            public const string AuthorizeFullfilment = "authorize-fulfillment";
            public const string OrderCreateError = "order-create-error";
            public const string OrderCreationError = "order-creation-error";
            public const string WindowToCancel = "window-to-cancel";
            public const string ReadyForHandling = "ready-for-handling";
            public const string StartHanding = "start-handling";
            public const string Handling = "handling";
            public const string InvoiceAfterCancellationDeny = "invoice-after-cancellation-deny";
            public const string OrderAccepted = "order-accepted";
            public const string Invoice = "invoice";
            public const string Invoiced = "invoiced";
            public const string Replaced = "replaced";
            public const string CancellationRequested = "cancellation-requested";
            public const string Cancel = "cancel";
            public const string Canceled = "canceled";
            public const string Cancelled = "cancelled";
        }

        public class Domain
        {
            public const string Fulfillment = "Fulfillment";
            public const string Marketplace = "Marketplace";
        }

        public class ShipStationOrderStatus
        {
            public const string AwaitingPayment = "awaiting_payment";
            public const string AwaitingShipment = "awaiting_shipment";
            public const string Shipped = "shipped";
            public const string OnHold = "on_hold";
            public const string Canceled = "cancelled";
        }

        public class ShipStationConfirmation
        {
            public const string None = "none";
            public const string Delivery = "delivery";
            public const string Signature = "signature";
            public const string AdultSignature = "adult_signature";
            public const string DirectSignature = "direct_signature";
        }

        //ORDER_NOTIFY, ITEM_ORDER_NOTIFY, SHIP_NOTIFY, ITEM_SHIP_NOTIFY
        public class WebhookEvent
        {
            public const string ORDER_NOTIFY = "ORDER_NOTIFY";
            public const string ITEM_ORDER_NOTIFY = "ITEM_ORDER_NOTIFY";
            public const string SHIP_NOTIFY = "SHIP_NOTIFY";
            public const string ITEM_SHIP_NOTIFY = "ITEM_SHIP_NOTIFY";

            public class FriendlyName
            {
                public const string ORDER_NOTIFY = "On New Orders";
                public const string ITEM_ORDER_NOTIFY = "On New Items";
                public const string SHIP_NOTIFY = "On Orders Shipped";
                public const string ITEM_SHIP_NOTIFY = "On Items Shipped";
            }
        }

        public class InvoiceType
        {
            /// <summary>
            /// The Output type should be used when the invoice you are sending is a selling invoice.
            /// </summary>
            public const string OUTPUT = "Output";

            /// <summary>
            /// The Input type should be used when you send a return invoice.
            /// </summary>
            public const string INPUT = "Input";
        }
    }
}
