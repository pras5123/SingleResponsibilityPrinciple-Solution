# SingleResponsibilityPrinciple-Solution

In my previous project ( SingleResponsibilityPriciple-Problem) We saw a problem in hte design of the Order class. This class had to change depending on the type of the order ( CreditCard Order / Online Order / Cash Order)

Instead of creating the single class (called Order), we need to create multiple sub classes for each Order ( namely : CreditCardOrder,OnlineOrder,CashOrder)

# Solution 

Final design will be like : 


public class CreditCardOrder  : Order
{}


public class OnlineOrder : Order
{}


public class CashOrder  : Order 
{}


# ----------------------------------------------------------
Order class will have a base function called 'Checkout' and this function takes different variant based on the type of the order ( namely : CreditCardOrder,OnlineOrder,CashOrder)
# 1
CreditCardOrder - deals with only Charging the card
# 2
OnlineOrder - deals with Reservation , Payment , Notification ( all these are a separate function to be injected to a particular class)
# 3
CashOrder  - Can inherit the basic functionality of Order class

# ----------------------------------------------------------
Take a look at this code to understand this better.


# ----------------------------------------------------------
Finally, with the Implementation of Order class where we only have a base functionality, so we don't need to change the 'Order' class. If we have a new functionality where we have to handle new type of Order, we just need to create a new class and inherit from the base class called ' Order'
