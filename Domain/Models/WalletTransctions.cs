using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class WalletTransctions
    {
        public WalletTransctions()
        {
            this.ArabicDescription=string.Empty;
            this.EnglishDescription=string.Empty;
        }
        public int Id { get;private set; }
        public string ArabicDescription { get;private set; }
        public string EnglishDescription { get;private set; }
        public decimal Amount { get;private set; }
        public bool Withdraw { get;private set; }
        public int? OrderId { get;private set; }
        public int CustomerId { get;private set; }



        public static Result<WalletTransctions> Instance(string arabicDescription,
                                                        string englishDescription,
                                                        int customerId,
                                                        decimal amount,
                                                        bool withDraw,
                                                        int? orderId)
        {
            if (string.IsNullOrEmpty(arabicDescription) || string.IsNullOrEmpty(englishDescription))
            {
                return Result.Failure<WalletTransctions>("Description is Required");
            }

            var transction = new WalletTransctions
            {
                Amount = amount,
                ArabicDescription = arabicDescription,
                EnglishDescription = englishDescription,
                CustomerId = customerId,
                OrderId = orderId,
                Withdraw = withDraw
            };

            return Result.Success<WalletTransctions>(transction);
        }
    }
}
