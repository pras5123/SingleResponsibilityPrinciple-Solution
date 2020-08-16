using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRPSolution
{
    class Program
    {
        static void Main(string[] args)
        {
            OrderItem objItem1 = new OrderItem { Identifier = "Toy", Quantity = 2 };
            OrderItem objItem2 = new OrderItem { Identifier = "Utensils", Quantity = 1 };
            List<OrderItem> items = new List<OrderItem>();
            items.Add(objItem1);
            items.Add(objItem2);
            ShoppingCart objCart = new ShoppingCart();
            objCart.CustomerEmail = "p.bhat@prowareness.nl";
            objCart.TotalAmount = 120;
            objCart.Items = items;
            //#1 : CashOrder
            CashOrder objOrder = new CashOrder(objCart);
            objOrder.Checkout();

            //#2 : CreditCardOrder
            PaymentDetails objPaymentDetails = new PaymentDetails();
            objPaymentDetails.CardholderName = "Prashanth";
            objPaymentDetails.CreditCardNumber = "1234-5678-6783-8712";
            objPaymentDetails.ExpiryDate = DateTime.Now.AddDays(30);
            IPaymentService objPaymentService = new PaymentService();            
            CreditCardOrder objCreditCardOrder = new CreditCardOrder(objCart, objPaymentDetails, objPaymentService);
            objCreditCardOrder.Checkout();

            //#3 : OnlineOrder
            INotificationService objNotificationService = new NotificationService();  //Filled with CustomerEmail
            IReservationService objReservationService = new ReservationService();     //Filled with items
            OnlineOrder objOnlineOrder = new OnlineOrder(objCart, objPaymentDetails, objNotificationService, objPaymentService, objReservationService);
            objOnlineOrder.Checkout();

        }
    }

    public class OrderItem
    {
        public string Identifier { get; set; }
        public int Quantity { get; set; }
    }
    public class ShoppingCart
    {
        public decimal TotalAmount { get; set; }
        public IEnumerable<OrderItem> Items { get; set; }
        public string CustomerEmail { get; set; }
    }
    public enum PaymentMethod
    {
        CreditCard
        , Cheque
    }
    public class PaymentDetails
    {
        public PaymentMethod PaymentMethod { get; set; }
        public string CreditCardNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string CardholderName { get; set; }
    }
    /*
     We know that we can process several types of Order: an online order, a cash order, a cheque order and possibly other types of Order 
     that we haven’t thought of. This calls for an abstract Order object:
     
     */
    public abstract class Order
    {
        private readonly ShoppingCart _shoppingCart;

        public Order(ShoppingCart shoppingCart)
        {
            _shoppingCart = shoppingCart;
        }

        public ShoppingCart ShoppingCart
        {
            get
            {
                return _shoppingCart;
            }
        }

        public virtual void Checkout()
        {
            //add common functionality to all Checkout operations
        }
    }

    public interface IReservationService
    {
        void ReserveInventory(IEnumerable<OrderItem> items);
    }

    public class ReservationService : IReservationService
    {
        public void ReserveInventory(IEnumerable<OrderItem> items)
        {
            //throw new NotImplementedException();
        }
    }

    public interface INotificationService
    {
        void NotifyCustomerOrderCreated(ShoppingCart cart);
    }

    public class NotificationService : INotificationService
    {

        public void NotifyCustomerOrderCreated(ShoppingCart cart)
        {
            string customerEmail = cart.CustomerEmail;
            if (!String.IsNullOrEmpty(customerEmail))
            {
                try
                {
                    //construct the email message and send it, implementation ignored
                }
                catch (Exception ex)
                {
                    //log the emailing error, implementation ignored
                }
            }
        }
    }

    public interface IPaymentService
    {
        void ProcessCreditCard(PaymentDetails paymentDetails, decimal moneyAmount);
    }

    public class PaymentService : IPaymentService
    {
        public string CardNumber { get; set; }
        public string Credentials { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string NameOnCard { get; set; }
        public decimal AmountToCharge { get; set; }
        public void ProcessCreditCard(PaymentDetails paymentDetails, decimal moneyAmount)
        {
            try
            {
                CardNumber = paymentDetails.CreditCardNumber;
                ExpiryDate = paymentDetails.ExpiryDate;
                NameOnCard = paymentDetails.CardholderName;
                AmountToCharge = moneyAmount;
                this.Charge();
            }
            catch (AccountBalanceMismatchException ex)
            {
                throw new OrderException("The card gateway rejected the card based on the address provided.", ex);
            }
            catch (Exception ex)
            {
                throw new OrderException("There was a problem with your card.", ex);
            }
        }
        public void Charge()
        {
            //throw new AccountBalanceMismatchException();
        }
    }

    public class AccountBalanceMismatchException : Exception
    {
    }
    public class OrderException : Exception
    {
        public OrderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
   

    /*
     when you go to a shop, place your goods into a real shopping cart and pay at the cashier. There’s no credit card process and no email notification. 
     Also, the inventory has probably been reduced when the goods were placed on the shelf, there’s no need to reduce the inventory further when the 
     actual purchase happens.That’s all for the cash order which represents an immediate purchase in a shop where the customer pays by cash. 
     */
    class CashOrder : Order
    {
        public CashOrder(ShoppingCart shoppingCart) 
            : base(shoppingCart)
        { 
        
        }
    }

    /*
     The credit card payment must be processed hence we’ll need a Payment service to take care of that. 
     We call upon its ProcessCreditCard method in the overridden Checkout method. Here the consumer platform can provide some concrete 
     implementation of the IPaymentService interface, it doesn’t matter to the Order object.     
     */

    public class CreditCardOrder : Order
    {
        private readonly PaymentDetails _paymentDetails;
        private readonly IPaymentService _paymentService;

        public CreditCardOrder(ShoppingCart shoppingCart, PaymentDetails paymentDetails, IPaymentService paymentService) 
            : base(shoppingCart)
        {
            _paymentDetails = paymentDetails;
            _paymentService = paymentService;
        }

        public override void Checkout()
        {
            _paymentService.ProcessCreditCard(_paymentDetails, ShoppingCart.TotalAmount);
            base.Checkout();
        }
    }

    /*
     Lastly we can have an online order with inventory management, payment service and email notifications:
     The consumer application will provide concrete implementations for the notification, inventory management and payment services. 
     The OnlineOrder object will not care what those implementations look like and will not be affected at all if you make a change in those 
     implementations or send in a different concrete implementation. As you can see these are the responsibilities that are likely to change over time. 
     However, the Order object and its concrete implementations won’t care any more
     */
    /*
     Also, note that we separated out the responsibilities into individual smaller interfaces and not a single large one with all responsibilities. 
     This follows the letter ‘I’ in solid, the Interface Segregation Principle, that we’ll look at in a future post.     
     */
    public class OnlineOrder : Order
    {
        private readonly INotificationService _notificationService;
        private readonly PaymentDetails _paymentDetails;
        private readonly IPaymentService _paymentService;
        private readonly IReservationService _reservationService;

        public OnlineOrder(ShoppingCart shoppingCart, PaymentDetails paymentDetails, INotificationService notificationService, 
            IPaymentService paymentService, IReservationService reservationService)
            : base(shoppingCart)
        {
            _paymentDetails = paymentDetails;
            _paymentService = paymentService;
            _reservationService = reservationService;
            _notificationService = notificationService;
        }
        public override void Checkout()
        {
            _paymentService.ProcessCreditCard(_paymentDetails, ShoppingCart.TotalAmount);
            _reservationService.ReserveInventory(ShoppingCart.Items);
            _notificationService.NotifyCustomerOrderCreated(ShoppingCart);
            base.Checkout();
        }
    }

    /*
     We are done with the refactoring, at least as far as SRP is concerned. We can still take up other areas of improvement such as making the Order 
     domain object cleaner by creating application services that will take care of the Checkout process. It may not be correct to put all these 
     services in a single domain object, but it depends on the philosophy you follow in your domain design. 
     That leads us to discussions on DDD (Domain Driven Design) which is not the scope of this post.
     
     */
}
