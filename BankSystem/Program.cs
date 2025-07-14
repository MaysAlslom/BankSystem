using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace BankSystem
{
    // Person
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public Person(string name, int age)
        {
            Name = name;
            Age = age;
        }

        public override string ToString()
        {
            return $"Name: {Name}, Age: {Age}";
        }
    }


    // Customer
    public class Customer : Person
    {
        public int AccountID { get; set; }
        public double Balance { get; set; }  // نقل الرصيد إلى Customer
        public Account CustomerAccount { get; set; }

        public Customer(string name, int age, int accountID) : base(name, age)
        {
            AccountID = accountID;
            Balance = 0;  // يبدأ الرصيد من 0
            CustomerAccount = new Account(accountID, this);  // ربط العميل بحسابه
        }

        // إيداع المال
        public void DepositMoney(double amount)
        {
            if (amount > 0)
            {
                Balance += amount;  // زيادة الرصيد
                                    // إضافة معاملة الإيداع إلى Transactions
                CustomerAccount.Deposit(amount);
            }
            else
            {
                Console.WriteLine("Invalid deposit amount.");
            }
        }

        // سحب المال
        public bool WithdrawMoney(double amount)
        {
            if (amount <= Balance)
            {
                Balance -= amount;  // خصم المبلغ من الرصيد
                                    // إضافة معاملة السحب إلى Transactions
                return CustomerAccount.Withdraw(amount);
            }
            else
            {
                Console.WriteLine("Insufficient balance.");
                return false;
            }
        }

        public double GetBalance()
        {
            return Balance;
        }

        public void SaveToFile()
        {
            string folderPath = @"C:\Users\Ms622\OneDrive\Desktop\BankSystem\Customers";
            string fileName = Path.Combine(folderPath, $"{this.AccountID}.txt");

            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // قراءة المعاملات السابقة من الملف
                List<Transaction> previousTransactions = new List<Transaction>();
                if (File.Exists(fileName))
                {
                    using (StreamReader reader = new StreamReader(fileName))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.StartsWith("Transaction ID:"))
                            {
                                var parts = line.Substring(15).Split(',');
                                if (parts.Length == 4)
                                {
                                    int transactionID = int.Parse(parts[0].Split(':')[1].Trim());
                                    string type = parts[1].Split(':')[1].Trim();
                                    double amount = double.Parse(parts[2].Split(':')[1].Trim());
                                    DateTime date = DateTime.Parse(parts[3].Split(':')[1].Trim());

                                    previousTransactions.Add(new Transaction(transactionID, amount, type));  // إضافة المعاملة السابقة
                                }
                            }
                        }
                    }
                }

                // إضافة المعاملات الجديدة إلى القائمة القديمة
                previousTransactions.AddRange(this.CustomerAccount.Transactions);

                // حفظ البيانات الجديدة (القديمة والجديدة)
                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    writer.WriteLine($"Name: {this.Name}");
                    writer.WriteLine($"Age: {this.Age}");
                    writer.WriteLine($"AccountID: {this.AccountID}");
                    writer.WriteLine($"Balance: {this.Balance}");

                    writer.WriteLine("Transactions:");
                    foreach (var transaction in previousTransactions)
                    {
                        writer.WriteLine($"Transaction ID: {transaction.TransactionID}, Type: {transaction.Type}, Amount: {transaction.Amount}, Date: {transaction.Date}");
                    }
                }

                Console.WriteLine($"Customer data saved to {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Account ID: {AccountID}, Balance: {Balance}";
        }
    }






    // فئة الحساب
    public class Account
    {
        public int AccountID { get; set; }
        public Customer Owner { get; set; }  // مالك الحساب
        public List<Transaction> Transactions { get; set; }  // قائمة المعاملات

        public Account(int accountID, Customer owner)
        {
            AccountID = accountID;
            Owner = owner;
            Transactions = new List<Transaction>();
        }

        // إيداع المال
        public void Deposit(double amount)
        {
            if (amount > 0)
            {
                // إضافة معاملة الإيداع
                Transactions.Add(new Transaction(Transactions.Count + 1, amount, "Deposit"));
            }
        }

        // سحب المال
        public bool Withdraw(double amount)
        {
            if (amount > 0)
            {
                // إضافة معاملة السحب
                Transactions.Add(new Transaction(Transactions.Count + 1, amount, "Withdrawal"));
                return true;
            }
            else
            {
                Console.WriteLine("Invalid withdrawal amount.");
                return false;
            }
        }

        public override string ToString()
        {
            return $"Account ID: {AccountID}, Transactions Count: {Transactions.Count}";
        }
    }


    // فئة المعاملات
    public class Transaction
    {
        public int TransactionID { get; set; }
        public double Amount { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }

        public Transaction(int transactionID, double amount, string type)
        {
            TransactionID = transactionID;
            Amount = amount;
            Type = type;
            Date = DateTime.Now;
        }

        public void DisplayTransaction()
        {
            Console.WriteLine($"Transaction ID: {TransactionID}, Type: {Type}, Amount: {Amount}, Date: {Date}");
        }
    }



    public class Admin
    {
        private BankSystem bankSystem;

        public Admin(BankSystem bankSystem)
        {
            this.bankSystem = bankSystem;
        }

        // عرض جميع الحسابات
        public void ViewAllAccounts()
        {
            var customers = bankSystem.GetAllCustomers();

            if (customers.Count == 0)
            {
                Console.WriteLine("No accounts found in the system.");
                return;  // خروج في حال عدم وجود حسابات
            }

            Console.WriteLine("\nAll Accounts:");
            foreach (var customer in customers)
            {
                Console.WriteLine(customer.ToString());
            }
        }

        // عرض جميع المعاملات
        public void ViewAllTransactions()
        {
            var customers = bankSystem.GetAllCustomers();

            if (customers.Count == 0)
            {
                Console.WriteLine("No accounts available to show transactions.");
                return;  // خروج في حال عدم وجود حسابات
            }

            Console.WriteLine("\nAll Transactions:");
            foreach (var customer in customers)
            {
                Console.WriteLine($"Account ID: {customer.AccountID} - {customer.Name} - Transactions:");
                foreach (var transaction in customer.CustomerAccount.Transactions)
                {
                    transaction.DisplayTransaction();
                }
            }
        }
    }







    public class BankSystem
    {
        private Dictionary<int, Customer> customersByAccountID; // لتخزين العملاء باستخدام رقم الحساب
        private Dictionary<string, Customer> customersByName; // لتخزين العملاء باستخدام اسم العميل
        private int nextAccountID;
        private string idFilePath = @"C:\Users\Ms622\OneDrive\Desktop\BankSystem\nextAccountID.txt";

        public BankSystem()
        {
            customersByAccountID = new Dictionary<int, Customer>();
            customersByName = new Dictionary<string, Customer>();
            nextAccountID = LoadNextAccountID();
        }

        // تحميل عملاء النظام من الملف
        public List<Customer> GetAllCustomers()
        {
            return customersByAccountID.Values.ToList();  // إعادة جميع العملاء الموجودين
        }

        // إضافة عميل جديد
        public Customer AddCustomer(string name, int age)
        {
            Customer newCustomer = new Customer(name, age, nextAccountID);
            nextAccountID++;

            // إضافة العميل إلى القاموس
            customersByAccountID.Add(newCustomer.AccountID, newCustomer);
            customersByName.Add(newCustomer.Name.ToLower(), newCustomer);

            Console.WriteLine($"Added new customer: {newCustomer.Name} with AccountID: {newCustomer.AccountID}");

            newCustomer.SaveToFile();  // حفظ العميل في الملف
            SaveNextAccountID();  // حفظ nextAccountID بعد إضافة العميل الجديد

            return newCustomer;
        }



        // إرجاع العميل باستخدام رقم الحساب
        public Customer GetCustomerByAccountID(int accountID)
        {
            if (customersByAccountID.ContainsKey(accountID))
            {
                return customersByAccountID[accountID];
            }
            else
            {
                Console.WriteLine("Account not found.");
                return null;
            }
        }

        // إرجاع العميل باستخدام اسم العميل
        public Customer GetCustomerByName(string name)
        {
            if (customersByName.ContainsKey(name.ToLower())) // تحويل الاسم إلى أحرف صغيرة لتجنب الأخطاء في البحث
            {
                return customersByName[name.ToLower()];
            }
            else
            {
                Console.WriteLine("Customer not found.");
                return null;
            }
        }

        // دالة لحفظ رقم الحساب التالي
        private void SaveNextAccountID()
        {
            try
            {
                File.WriteAllText(idFilePath, nextAccountID.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving nextAccountID: {ex.Message}");
            }
        }

        // دالة لتحميل رقم الحساب التالي من الملف
        private int LoadNextAccountID()
        {
            try
            {
                if (File.Exists(idFilePath))
                {
                    string id = File.ReadAllText(idFilePath);
                    return int.Parse(id);
                }
                else
                {
                    return 100;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading nextAccountID: {ex.Message}");
                return 100;
            }
        }
    }







    public class Program
    {
        static void Main(string[] args)
        {
            BankSystem bankSystem = new BankSystem();
            Admin admin = new Admin(bankSystem);
            bool exitProgram = false;  // للتحكم في إنهاء البرنامج

            while (!exitProgram)
            {
                Console.WriteLine("Welcome to the Bank System!");
                Console.WriteLine("1. Create a new account");
                Console.WriteLine("2. Use an existing account");
                Console.WriteLine("3. Admin Options");
                Console.WriteLine("4. Exit");

                int choice = int.Parse(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        // إنشاء حساب جديد
                        Console.WriteLine("Enter customer name: ");
                        string customerName = Console.ReadLine();

                        Console.WriteLine("Enter customer age: ");
                        int customerAge = int.Parse(Console.ReadLine());

                        Customer newCustomer = bankSystem.AddCustomer(customerName, customerAge);

                        Console.WriteLine($"Account created for {newCustomer.Name} with Account ID: {newCustomer.AccountID}");
                        newCustomer.SaveToFile();
                        Console.WriteLine("Your account has been successfully created.");

                        PerformOperations(newCustomer, bankSystem);
                        break;

                    case 2:
                        // استخدام حساب موجود
                        Console.WriteLine("Enter Account ID to begin operations: ");
                        int accountID = int.Parse(Console.ReadLine());

                        Customer customer = bankSystem.GetCustomerByAccountID(accountID);

                        if (customer != null)
                        {
                            PerformOperations(customer, bankSystem);
                        }
                        else
                        {
                            Console.WriteLine("Account not found.");
                        }
                        break;

                    case 3:
                        // خيارات الإدارة
                        Console.WriteLine("Admin Options:");
                        Console.WriteLine("1. View all accounts");
                        Console.WriteLine("2. View all transactions");

                        int adminChoice = int.Parse(Console.ReadLine());

                        if (adminChoice == 1)
                        {
                            admin.ViewAllAccounts();  // عرض جميع الحسابات
                        }
                        else if (adminChoice == 2)
                        {
                            admin.ViewAllTransactions();  // عرض جميع المعاملات
                        }
                        break;

                    case 4:
                        // الخروج من البرنامج
                        exitProgram = true;
                        Console.WriteLine("Exiting the Bank System. Thank you!");
                        break;

                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
            }
        }

        static void PerformOperations(Customer customer, BankSystem bankSystem)
        {
            bool exit = false;

            while (!exit)
            {
                Console.WriteLine("\nChoose an operation:");
                Console.WriteLine("1. Deposit Money");
                Console.WriteLine("2. Withdraw Money");
                Console.WriteLine("3. View Previous Transactions");
                Console.WriteLine("4. View Balance");
                Console.WriteLine("5. Filter Transactions by Date or Type");
                Console.WriteLine("6. Exit");

                int choice = int.Parse(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        // إيداع المال
                        Console.WriteLine("Enter deposit amount: ");
                        double depositAmount = double.Parse(Console.ReadLine());
                        customer.DepositMoney(depositAmount);
                        Console.WriteLine($"New Balance: {customer.GetBalance()}");
                        customer.SaveToFile(); // حفظ البيانات في الملف بعد العملية
                        break;

                    case 2:
                        // سحب المال
                        Console.WriteLine("Enter withdrawal amount: ");
                        double withdrawalAmount = double.Parse(Console.ReadLine());
                        bool success = customer.WithdrawMoney(withdrawalAmount);
                        if (success)
                        {
                            Console.WriteLine($"New Balance: {customer.GetBalance()}");
                            customer.SaveToFile(); // حفظ البيانات في الملف بعد العملية
                        }
                        else
                        {
                            Console.WriteLine("Withdrawal failed due to insufficient funds.");
                        }
                        break;

                    case 3:
                        // عرض العمليات السابقة
                        Console.WriteLine("Previous Transactions:");
                        foreach (var transaction in customer.CustomerAccount.Transactions)
                        {
                            transaction.DisplayTransaction();  // عرض كل معاملة تم إجراؤها
                        }
                        break;

                    case 4:
                        // عرض الرصيد
                        Console.WriteLine($"Your balance is: {customer.GetBalance()}");
                        break;

                    case 5:
                        // Filter transactions by Date or Type
                        Console.WriteLine("Filter Transactions by:");
                        Console.WriteLine("1. Date");
                        Console.WriteLine("2. Type");
                        int filterChoice = int.Parse(Console.ReadLine());

                        if (filterChoice == 1)
                        {
                            // Filter by Year and Month
                            Console.WriteLine("Enter Year (yyyy): ");
                            int year = int.Parse(Console.ReadLine());

                            Console.WriteLine("Enter Month (1-12): ");
                            int month = int.Parse(Console.ReadLine());

                            var filteredTransactions = customer.CustomerAccount.Transactions
                                .Where(t => t.Date.Year == year && t.Date.Month == month)
                                .ToList();

                            Console.WriteLine($"Filtered Transactions for {year}-{month:D2}:");
                            if (filteredTransactions.Count > 0)
                            {
                                foreach (var transaction in filteredTransactions)
                                {
                                    transaction.DisplayTransaction();
                                }
                            }
                            else
                            {
                                Console.WriteLine("No transactions found for the selected month.");
                            }
                        }
                        else if (filterChoice == 2)
                        {
                            // Handle filtering by type (Deposit or Withdrawal)
                            Console.WriteLine("Enter transaction type (Deposit/Withdrawal): ");
                            string type = Console.ReadLine();
                            var filteredByType = customer.CustomerAccount.Transactions
                                .Where(t => t.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
                                .ToList();

                            Console.WriteLine($"Filtered Transactions of type {type}:");
                            if (filteredByType.Count > 0)
                            {
                                foreach (var transaction in filteredByType)
                                {
                                    transaction.DisplayTransaction();
                                }
                            }
                            else
                            {
                                Console.WriteLine("No transactions found for the selected type.");
                            }
                        }
                        break;


                    case 6:
                        // الخروج
                        exit = true;
                        Console.WriteLine("Thank you for using the bank system!");
                        break;

                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }

                if (!exit)
                {
                    // سؤالك إذا كان يريد إجراء عملية أخرى أو إنهاء
                    Console.WriteLine("\nWould you like to perform another operation? (y/n)");
                    string continueChoice = Console.ReadLine().ToLower();

                    if (continueChoice == "n")
                    {
                        exit = true; // إذا اختار "n" ينهي العملية
                        Console.WriteLine("Exiting the system. Thank you!");
                    }
                }
            }
        }


    }
}
