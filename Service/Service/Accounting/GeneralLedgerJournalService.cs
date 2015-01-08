using Core.DomainModel;
using Core.Interface.Repository;
using Core.Interface.Service;
using Core.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Interface.Validation;

namespace Service.Service
{
    public class GeneralLedgerJournalService : IGeneralLedgerJournalService
    {
        private IGeneralLedgerJournalRepository _repository;
        private IGeneralLedgerJournalValidator _validator;

        public GeneralLedgerJournalService(IGeneralLedgerJournalRepository _generalLedgerJournalRepository, IGeneralLedgerJournalValidator _generalLedgerJournalValidator)
        {
            _repository = _generalLedgerJournalRepository;
            _validator = _generalLedgerJournalValidator;
        }

        public IGeneralLedgerJournalValidator GetValidator()
        {
            return _validator;
        }

        public IQueryable<GeneralLedgerJournal> GetQueryable()
        {
            return _repository.GetQueryable();
        }

        public IList<GeneralLedgerJournal> GetAll()
        {
            return _repository.GetAll();
        }

        public IList<GeneralLedgerJournal> GetObjectsByAccountId(int accountId)
        {
            return _repository.GetObjectsByAccountId(accountId);
        }

        public GeneralLedgerJournal GetObjectById(int Id)
        {
            return _repository.GetObjectById(Id);
        }

        public IList<GeneralLedgerJournal> GetObjectsBySourceDocument(int accountId, string SourceDocument, int SourceDocumentId)
        {
            return _repository.GetObjectsBySourceDocument(accountId, SourceDocument, SourceDocumentId);
        }

        public GeneralLedgerJournal CreateObject(GeneralLedgerJournal generalLedgerJournal, IAccountService _accountService)
        {
            generalLedgerJournal.Errors = new Dictionary<String, String>();
            return (_validator.ValidCreateObject(generalLedgerJournal, _accountService) ? _repository.CreateObject(generalLedgerJournal) : generalLedgerJournal);
        }

        public GeneralLedgerJournal SoftDeleteObject(GeneralLedgerJournal generalLedgerJournal)
        {
            return (_validator.ValidDeleteObject(generalLedgerJournal) ? _repository.SoftDeleteObject(generalLedgerJournal) : generalLedgerJournal);
        }

        public bool DeleteObject(int Id)
        {
            return _repository.DeleteObject(Id);
        }

        public IList<GeneralLedgerJournal> CreateConfirmationJournalForMemorial(Memorial memorial, IMemorialDetailService _memorialDetailService, IAccountService _accountService)
        {
            // User Input Memorial
            #region User Input Memorial

            IList<MemorialDetail> details = _memorialDetailService.GetObjectsByMemorialId(memorial.Id);
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();

            foreach (var memorialDetail in details)
            {
                GeneralLedgerJournal journal = new GeneralLedgerJournal()
                {
                    AccountId = memorialDetail.AccountId,
                    SourceDocument = Constant.GeneralLedgerSource.Memorial,
                    SourceDocumentId = memorial.Id,
                    TransactionDate = (DateTime)memorial.ConfirmationDate,
                    Status = memorialDetail.Status,
                    Amount = memorialDetail.Amount
                };
                journal = CreateObject(journal, _accountService);
                journals.Add(journal);
            }

            return journals;
            #endregion
        }

        public IList<GeneralLedgerJournal> CreateUnconfirmationJournalForMemorial(Memorial memorial, DateTime UnconfirmationDate, IMemorialDetailService _memorialDetailService, IAccountService _accountService)
        {
            // Use Input Memorial
            #region User Input Memorial

            //DateTime UnconfirmationDate = memorial.ConfirmationDate.GetValueOrDefault(); // DateTime.Now;
            IList<MemorialDetail> details = _memorialDetailService.GetObjectsByMemorialId(memorial.Id);
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();

            foreach (var memorialDetail in details)
            {
                GeneralLedgerJournal journal = new GeneralLedgerJournal()
                {
                    AccountId = memorialDetail.AccountId,
                    SourceDocument = Constant.GeneralLedgerSource.Memorial,
                    SourceDocumentId = memorial.Id,
                    TransactionDate = UnconfirmationDate,
                    Status = (memorialDetail.Status == Constant.GeneralLedgerStatus.Debit) ? Constant.GeneralLedgerStatus.Credit : Constant.GeneralLedgerStatus.Debit,
                    Amount = memorialDetail.Amount
                };
                journal = CreateObject(journal, _accountService);
                journals.Add(journal);
            }

            return journals;
            #endregion
        }

        public IList<GeneralLedgerJournal> CreateConfirmationJournalForPaymentRequest(PaymentRequest paymentRequest, IPaymentRequestDetailService _paymentRequestDetailService, IAccountService _accountService)
        {
            // Credit AccountPayableNonTrading, Debit User Input
            #region Credit AccountPayableNonTrading, Debit User Input

            IList<PaymentRequestDetail> details = _paymentRequestDetailService.GetObjectsByPaymentRequestId(paymentRequest.Id);
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();

            foreach (var paymentRequestDetail in details)
            {
                GeneralLedgerJournal journal = new GeneralLedgerJournal()
                {
                    AccountId = paymentRequestDetail.AccountId,
                    SourceDocument = Constant.GeneralLedgerSource.PaymentRequest,
                    SourceDocumentId = paymentRequest.Id,
                    TransactionDate = (DateTime)paymentRequest.ConfirmationDate,
                    Status = paymentRequestDetail.Status,
                    Amount = paymentRequestDetail.Amount
                };
                journal = CreateObject(journal, _accountService);
                journals.Add(journal);
            }

            return journals;
            #endregion
        }

        public IList<GeneralLedgerJournal> CreateUnconfirmationJournalForPaymentRequest(PaymentRequest paymentRequest, DateTime UnconfirmationDate, IPaymentRequestDetailService _paymentRequestDetailService, IAccountService _accountService)
        {
            // Debit AccountPayableNonTrading, Credit User Input
            #region Debit AccountPayableNonTrading, Credit User Input

            IList<PaymentRequestDetail> details = _paymentRequestDetailService.GetObjectsByPaymentRequestId(paymentRequest.Id);
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            //DateTime UnconfirmationDate = paymentRequest.ConfirmationDate.GetValueOrDefault(); // DateTime.Now;

            foreach (var paymentRequestDetail in details)
            {
                GeneralLedgerJournal journal = new GeneralLedgerJournal()
                {
                    AccountId = paymentRequestDetail.AccountId,
                    SourceDocument = Constant.GeneralLedgerSource.PaymentRequest,
                    SourceDocumentId = paymentRequest.Id,
                    TransactionDate = UnconfirmationDate,
                    Status = (paymentRequestDetail.Status == Constant.GeneralLedgerStatus.Debit) ? Constant.GeneralLedgerStatus.Credit : Constant.GeneralLedgerStatus.Debit,
                    Amount = paymentRequestDetail.Amount
                };
                journal = CreateObject(journal, _accountService);
                journals.Add(journal);
            }

            return journals;
            #endregion
        }

        public IList<GeneralLedgerJournal> CreateConfirmationJournalForPaymentVoucherNonTrading(PaymentVoucher paymentVoucher, CashBank cashBank, decimal Amount, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();

            GeneralLedgerJournal debitaccountpayable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountPayableNonTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.PaymentVoucher,
                SourceDocumentId = paymentVoucher.Id,
                TransactionDate = (DateTime)paymentVoucher.ConfirmationDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = Amount, // paymentVoucher.TotalAmount
            };
            debitaccountpayable = CreateObject(debitaccountpayable, _accountService);

            if (paymentVoucher.IsGBCH)
            {
                GeneralLedgerJournal creditgbch = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.GBCHPayable).Id, // + cashBank.Id.ToString("D3")).Id,
                    SourceDocument = Constant.GeneralLedgerSource.PaymentVoucher,
                    SourceDocumentId = paymentVoucher.Id,
                    TransactionDate = (DateTime)paymentVoucher.ConfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Credit,
                    Amount = Amount, // paymentVoucher.TotalAmount
                };
                creditgbch = CreateObject(creditgbch, _accountService);
                journals.Add(creditgbch);
            }
            else
            {
                GeneralLedgerJournal creditcashbank = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + cashBank.Id.ToString("D3")).Id,
                    SourceDocument = Constant.GeneralLedgerSource.PaymentVoucher,
                    SourceDocumentId = paymentVoucher.Id,
                    TransactionDate = (DateTime)paymentVoucher.ConfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Credit,
                    Amount = Amount, // paymentVoucher.TotalAmount
                };
                creditcashbank = CreateObject(creditcashbank, _accountService);
                journals.Add(creditcashbank);
            }
            journals.Add(debitaccountpayable);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateUnconfirmationJournalForPaymentVoucherNonTrading(PaymentVoucher paymentVoucher, CashBank cashBank, DateTime UnconfirmationDate, decimal Amount, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            //DateTime UnconfirmationDate = paymentVoucher.ConfirmationDate.GetValueOrDefault(); // DateTime.Now;

            GeneralLedgerJournal creditaccountpayable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountPayableNonTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.PaymentVoucher,
                SourceDocumentId = paymentVoucher.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = Amount, // paymentVoucher.TotalAmount
            };
            creditaccountpayable = CreateObject(creditaccountpayable, _accountService);

            if (paymentVoucher.IsGBCH)
            {
                GeneralLedgerJournal debitgbch = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.GBCHPayable).Id, // + cashBank.Id.ToString("D3")).Id,
                    SourceDocument = Constant.GeneralLedgerSource.PaymentVoucher,
                    SourceDocumentId = paymentVoucher.Id,
                    TransactionDate = UnconfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Debit,
                    Amount = Amount, // paymentVoucher.TotalAmount
                };
                debitgbch = CreateObject(debitgbch, _accountService);
                journals.Add(debitgbch);
            }
            else
            {
                GeneralLedgerJournal debitcashbank = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + cashBank.Id.ToString("D3")).Id,
                    SourceDocument = Constant.GeneralLedgerSource.PaymentVoucher,
                    SourceDocumentId = paymentVoucher.Id,
                    TransactionDate = UnconfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Debit,
                    Amount = Amount, // paymentVoucher.TotalAmount
                };
                debitcashbank = CreateObject(debitcashbank, _accountService);
                journals.Add(debitcashbank);
            }
            journals.Add(creditaccountpayable);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateReconcileJournalForPaymentVoucher(PaymentVoucher paymentVoucher, CashBank cashBank, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();

            GeneralLedgerJournal debitaccountpayable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.GBCHPayable).Id,
                SourceDocument = Constant.GeneralLedgerSource.PaymentVoucher,
                SourceDocumentId = paymentVoucher.Id,
                TransactionDate = (DateTime)paymentVoucher.ReconciliationDate.GetValueOrDefault(), // .ConfirmationDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = paymentVoucher.TotalAmount
            };
            debitaccountpayable = CreateObject(debitaccountpayable, _accountService);

            GeneralLedgerJournal creditcashbank = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + cashBank.Id.ToString("D3")).Id,
                SourceDocument = Constant.GeneralLedgerSource.PaymentVoucher,
                SourceDocumentId = paymentVoucher.Id,
                TransactionDate = (DateTime)paymentVoucher.ReconciliationDate.GetValueOrDefault(), // .ConfirmationDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = paymentVoucher.TotalAmount
            };
            creditcashbank = CreateObject(creditcashbank, _accountService);

            journals.Add(debitaccountpayable);
            journals.Add(creditcashbank);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateUnreconcileJournalForPaymentVoucher(PaymentVoucher paymentVoucher, CashBank cashBank, DateTime UnreconcileDate, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            //DateTime UnreconcileDate = paymentVoucher.ReconciliationDate.GetValueOrDefault(); // DateTime.Now;

            GeneralLedgerJournal creditaccountpayable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.GBCHPayable).Id,
                SourceDocument = Constant.GeneralLedgerSource.PaymentVoucher,
                SourceDocumentId = paymentVoucher.Id,
                TransactionDate = UnreconcileDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = paymentVoucher.TotalAmount
            };
            creditaccountpayable = CreateObject(creditaccountpayable, _accountService);

            GeneralLedgerJournal debitcashbank = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + cashBank.Id.ToString("D3")).Id,
                SourceDocument = Constant.GeneralLedgerSource.PaymentVoucher,
                SourceDocumentId = paymentVoucher.Id,
                TransactionDate = UnreconcileDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = paymentVoucher.TotalAmount
            };
            debitcashbank = CreateObject(debitcashbank, _accountService);

            journals.Add(creditaccountpayable);
            journals.Add(debitcashbank);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateConfirmationJournalForPaymentVoucherTrading(PaymentVoucher paymentVoucher, CashBank cashBank, decimal Amount, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();

            GeneralLedgerJournal debitaccountpayable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountPayableTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.PaymentVoucher,
                SourceDocumentId = paymentVoucher.Id,
                TransactionDate = (DateTime)paymentVoucher.ConfirmationDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = Amount, // paymentVoucher.TotalAmount
            };
            debitaccountpayable = CreateObject(debitaccountpayable, _accountService);

            if (paymentVoucher.IsGBCH)
            {
                GeneralLedgerJournal creditgbch = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.GBCHPayable).Id, // + cashBank.Id.ToString("D3")).Id,
                    SourceDocument = Constant.GeneralLedgerSource.PaymentVoucher,
                    SourceDocumentId = paymentVoucher.Id,
                    TransactionDate = (DateTime)paymentVoucher.ConfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Credit,
                    Amount = Amount, // paymentVoucher.TotalAmount
                };
                creditgbch = CreateObject(creditgbch, _accountService);
                journals.Add(creditgbch);
            }
            else
            {
                GeneralLedgerJournal creditcashbank = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + cashBank.Id.ToString("D3")).Id,
                    SourceDocument = Constant.GeneralLedgerSource.PaymentVoucher,
                    SourceDocumentId = paymentVoucher.Id,
                    TransactionDate = (DateTime)paymentVoucher.ConfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Credit,
                    Amount = Amount, // paymentVoucher.TotalAmount
                };
                creditcashbank = CreateObject(creditcashbank, _accountService);
                journals.Add(creditcashbank);
            }
            journals.Add(debitaccountpayable);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateUnconfirmationJournalForPaymentVoucherTrading(PaymentVoucher paymentVoucher, CashBank cashBank, DateTime UnconfirmationDate, decimal Amount, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            //DateTime UnconfirmationDate = paymentVoucher.ConfirmationDate.GetValueOrDefault(); // DateTime.Now;

            GeneralLedgerJournal creditaccountpayable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountPayableTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.PaymentVoucher,
                SourceDocumentId = paymentVoucher.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = Amount, // paymentVoucher.TotalAmount
            };
            creditaccountpayable = CreateObject(creditaccountpayable, _accountService);

            if (paymentVoucher.IsGBCH)
            {
                GeneralLedgerJournal debitgbch = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.GBCHPayable).Id, // + cashBank.Id.ToString("D3")).Id,
                    SourceDocument = Constant.GeneralLedgerSource.PaymentVoucher,
                    SourceDocumentId = paymentVoucher.Id,
                    TransactionDate = UnconfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Debit,
                    Amount = Amount, // paymentVoucher.TotalAmount
                };
                debitgbch = CreateObject(debitgbch, _accountService);
                journals.Add(debitgbch);
            }
            else
            {
                GeneralLedgerJournal debitcashbank = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + cashBank.Id.ToString("D3")).Id,
                    SourceDocument = Constant.GeneralLedgerSource.PaymentVoucher,
                    SourceDocumentId = paymentVoucher.Id,
                    TransactionDate = UnconfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Debit,
                    Amount = Amount, // paymentVoucher.TotalAmount
                };
                debitcashbank = CreateObject(debitcashbank, _accountService);
                journals.Add(debitcashbank);
            }
            journals.Add(creditaccountpayable);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateConfirmationJournalForReceiptVoucherTrading(ReceiptVoucher receiptVoucher, CashBank cashBank, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();

            GeneralLedgerJournal creditaccountreceivable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountReceivableTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.ReceiptVoucher,
                SourceDocumentId = receiptVoucher.Id,
                TransactionDate = (DateTime)receiptVoucher.ConfirmationDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = receiptVoucher.TotalAmount
            };
            creditaccountreceivable = CreateObject(creditaccountreceivable, _accountService);

            if (receiptVoucher.IsGBCH)
            {
                GeneralLedgerJournal debitgbch = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.GBCHReceivable).Id, // + cashBank.Id.ToString("D3")).Id,
                    SourceDocument = Constant.GeneralLedgerSource.ReceiptVoucher,
                    SourceDocumentId = receiptVoucher.Id,
                    TransactionDate = (DateTime)receiptVoucher.ConfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Debit,
                    Amount = receiptVoucher.TotalAmount
                };
                debitgbch = CreateObject(debitgbch, _accountService);
                journals.Add(debitgbch);
            }
            else
            {
                GeneralLedgerJournal debitcashbank = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + cashBank.Id.ToString("D3")).Id,
                    SourceDocument = Constant.GeneralLedgerSource.ReceiptVoucher,
                    SourceDocumentId = receiptVoucher.Id,
                    TransactionDate = (DateTime)receiptVoucher.ConfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Debit,
                    Amount = receiptVoucher.TotalAmount
                };
                debitcashbank = CreateObject(debitcashbank, _accountService);
                journals.Add(debitcashbank);
            }
            journals.Add(creditaccountreceivable);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateUnconfirmationJournalForReceiptVoucherTrading(ReceiptVoucher receiptVoucher, CashBank cashBank, DateTime UnconfirmationDate, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            //DateTime UnconfirmationDate = receiptVoucher.ConfirmationDate.GetValueOrDefault(); // DateTime.Now;

            GeneralLedgerJournal debitaccountreceivable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountReceivableTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.ReceiptVoucher,
                SourceDocumentId = receiptVoucher.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = receiptVoucher.TotalAmount
            };
            debitaccountreceivable = CreateObject(debitaccountreceivable, _accountService);

            if (receiptVoucher.IsGBCH)
            {
                GeneralLedgerJournal creditgbch = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.GBCHReceivable).Id, // + cashBank.Id.ToString("D3")).Id,
                    SourceDocument = Constant.GeneralLedgerSource.ReceiptVoucher,
                    SourceDocumentId = receiptVoucher.Id,
                    TransactionDate = UnconfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Credit,
                    Amount = receiptVoucher.TotalAmount
                };
                creditgbch = CreateObject(creditgbch, _accountService);
                journals.Add(creditgbch);
            }
            else
            {
                GeneralLedgerJournal creditcashbank = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + cashBank.Id.ToString("D3")).Id,
                    SourceDocument = Constant.GeneralLedgerSource.ReceiptVoucher,
                    SourceDocumentId = receiptVoucher.Id,
                    TransactionDate = UnconfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Credit,
                    Amount = receiptVoucher.TotalAmount
                };
                creditcashbank = CreateObject(creditcashbank, _accountService);
                journals.Add(creditcashbank);
            }
            journals.Add(debitaccountreceivable);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateReconcileJournalForReceiptVoucher(ReceiptVoucher receiptVoucher, CashBank cashBank, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();

            GeneralLedgerJournal debitcashbank = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + cashBank.Id.ToString("D3")).Id,
                SourceDocument = Constant.GeneralLedgerSource.ReceiptVoucher,
                SourceDocumentId = receiptVoucher.Id,
                TransactionDate = (DateTime)receiptVoucher.ReconciliationDate.GetValueOrDefault(),
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = receiptVoucher.TotalAmount
            };
            debitcashbank = CreateObject(debitcashbank, _accountService);

            GeneralLedgerJournal creditaccountreceivable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.GBCHReceivable).Id,
                SourceDocument = Constant.GeneralLedgerSource.ReceiptVoucher,
                SourceDocumentId = receiptVoucher.Id,
                TransactionDate = (DateTime)receiptVoucher.ReconciliationDate.GetValueOrDefault(),
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = receiptVoucher.TotalAmount
            };
            creditaccountreceivable = CreateObject(creditaccountreceivable, _accountService);

            journals.Add(debitcashbank);
            journals.Add(creditaccountreceivable);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateUnreconcileJournalForReceiptVoucher(ReceiptVoucher receiptVoucher, CashBank cashBank, DateTime UnreconcileDate, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            //DateTime UnreconcileDate = receiptVoucher.ConfirmationDate.GetValueOrDefault(); // DateTime.Now;

            GeneralLedgerJournal creditcashbank = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + cashBank.Id.ToString("D3")).Id,
                SourceDocument = Constant.GeneralLedgerSource.ReceiptVoucher,
                SourceDocumentId = receiptVoucher.Id,
                TransactionDate = UnreconcileDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = receiptVoucher.TotalAmount
            };
            creditcashbank = CreateObject(creditcashbank, _accountService);

            GeneralLedgerJournal debitaccountreceivable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.GBCHReceivable).Id,
                SourceDocument = Constant.GeneralLedgerSource.ReceiptVoucher,
                SourceDocumentId = receiptVoucher.Id,
                TransactionDate = UnreconcileDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = receiptVoucher.TotalAmount
            };
            debitaccountreceivable = CreateObject(debitaccountreceivable, _accountService);

            journals.Add(creditcashbank);
            journals.Add(debitaccountreceivable);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateConfirmationJournalForCashBankAdjustment(CashBankAdjustment cashBankAdjustment, CashBank cashBank, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();

            if (cashBankAdjustment.Amount >= 0)
            {
                string LegacyCode = Constant.AccountLegacyCode.CashBank + cashBank.Id.ToString("D3");
                int AccountId = _accountService.GetObjectByLegacyCode(LegacyCode).Id;
                GeneralLedgerJournal debitcashbank = new GeneralLedgerJournal()
                {
                    AccountId = AccountId,
                    SourceDocument = Constant.GeneralLedgerSource.CashBankAdjustment,
                    SourceDocumentId = cashBankAdjustment.Id,
                    TransactionDate = (DateTime)cashBankAdjustment.ConfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Debit,
                    Amount = cashBankAdjustment.Amount
                };
                debitcashbank = CreateObject(debitcashbank, _accountService);

                GeneralLedgerJournal creditcashbankequityadjustment = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.EquityAdjustment).Id,
                    SourceDocument = Constant.GeneralLedgerSource.CashBankAdjustment,
                    SourceDocumentId = cashBankAdjustment.Id,
                    TransactionDate = (DateTime)cashBankAdjustment.ConfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Credit,
                    Amount = cashBankAdjustment.Amount
                };
                creditcashbankequityadjustment = CreateObject(creditcashbankequityadjustment, _accountService);

                journals.Add(debitcashbank);
                journals.Add(creditcashbankequityadjustment);
            }
            else
            {
                GeneralLedgerJournal debitcashbankadjustmentexpense = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBankAdjustmentExpense).Id,
                    SourceDocument = Constant.GeneralLedgerSource.CashBankAdjustment,
                    SourceDocumentId = cashBankAdjustment.Id,
                    TransactionDate = (DateTime)cashBankAdjustment.ConfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Debit,
                    Amount = Math.Abs(cashBankAdjustment.Amount)
                };
                debitcashbankadjustmentexpense = CreateObject(debitcashbankadjustmentexpense, _accountService);

                GeneralLedgerJournal creditcashbank = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + cashBank.Id.ToString("D3")).Id,
                    SourceDocument = Constant.GeneralLedgerSource.CashBankAdjustment,
                    SourceDocumentId = cashBankAdjustment.Id,
                    TransactionDate = (DateTime)cashBankAdjustment.ConfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Credit,
                    Amount = Math.Abs(cashBankAdjustment.Amount)
                };
                creditcashbank = CreateObject(creditcashbank, _accountService);

                journals.Add(debitcashbankadjustmentexpense);
                journals.Add(creditcashbank);
            }

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateUnconfirmationJournalForCashBankAdjustment(CashBankAdjustment cashBankAdjustment, CashBank cashBank, DateTime UnconfirmationDate, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            //DateTime UnconfirmationDate = cashBankAdjustment.ConfirmationDate.GetValueOrDefault(); // DateTime.Now;

            if (cashBankAdjustment.Amount >= 0)
            {
                GeneralLedgerJournal creditcashbank = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + cashBank.Id.ToString("D3")).Id,
                    SourceDocument = Constant.GeneralLedgerSource.CashBankAdjustment,
                    SourceDocumentId = cashBankAdjustment.Id,
                    TransactionDate = UnconfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Credit,
                    Amount = cashBankAdjustment.Amount
                };
                creditcashbank = CreateObject(creditcashbank, _accountService);

                GeneralLedgerJournal debitcashbankequityadjustment = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.EquityAdjustment).Id,
                    SourceDocument = Constant.GeneralLedgerSource.CashBankAdjustment,
                    SourceDocumentId = cashBankAdjustment.Id,
                    TransactionDate = UnconfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Debit,
                    Amount = cashBankAdjustment.Amount
                };
                debitcashbankequityadjustment = CreateObject(debitcashbankequityadjustment, _accountService);

                journals.Add(creditcashbank);
                journals.Add(debitcashbankequityadjustment);
            }
            else
            {
                GeneralLedgerJournal creditcashbankadjustmentexpense = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBankAdjustmentExpense).Id,
                    SourceDocument = Constant.GeneralLedgerSource.CashBankAdjustment,
                    SourceDocumentId = cashBankAdjustment.Id,
                    TransactionDate = UnconfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Credit,
                    Amount = Math.Abs(cashBankAdjustment.Amount)
                };
                creditcashbankadjustmentexpense = CreateObject(creditcashbankadjustmentexpense, _accountService);

                GeneralLedgerJournal debitcashbank = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + cashBank.Id.ToString("D3")).Id,
                    SourceDocument = Constant.GeneralLedgerSource.CashBankAdjustment,
                    SourceDocumentId = cashBankAdjustment.Id,
                    TransactionDate = UnconfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Debit,
                    Amount = Math.Abs(cashBankAdjustment.Amount)
                };
                debitcashbank = CreateObject(debitcashbank, _accountService);

                journals.Add(creditcashbankadjustmentexpense);
                journals.Add(debitcashbank);
            }

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateConfirmationJournalForCashBankMutation(CashBankMutation cashBankMutation, CashBank sourceCashBank, CashBank targetCashBank, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();

            GeneralLedgerJournal debittargetcashbank = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + targetCashBank.Id.ToString("D3")).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashBankMutation,
                SourceDocumentId = cashBankMutation.Id,
                TransactionDate = (DateTime)cashBankMutation.ConfirmationDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = cashBankMutation.Amount
            };
            debittargetcashbank = CreateObject(debittargetcashbank, _accountService);

            GeneralLedgerJournal creditsourcecashbank = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + sourceCashBank.Id.ToString("D3")).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashBankMutation,
                SourceDocumentId = cashBankMutation.Id,
                TransactionDate = (DateTime)cashBankMutation.ConfirmationDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = cashBankMutation.Amount
            };
            creditsourcecashbank = CreateObject(creditsourcecashbank, _accountService);

            journals.Add(debittargetcashbank);
            journals.Add(creditsourcecashbank);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateUnconfirmationJournalForCashBankMutation(CashBankMutation cashBankMutation, CashBank sourceCashBank, CashBank targetCashBank, DateTime UnconfirmationDate, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            //DateTime UnconfirmationDate = cashBankMutation.ConfirmationDate.GetValueOrDefault(); // DateTime.Now;

            GeneralLedgerJournal credittargetcashbank = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + targetCashBank.Id.ToString("D3")).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashBankMutation,
                SourceDocumentId = cashBankMutation.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = cashBankMutation.Amount
            };
            credittargetcashbank = CreateObject(credittargetcashbank, _accountService);

            GeneralLedgerJournal debitsourcecashbank = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.CashBank + sourceCashBank.Id.ToString("D3")).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashBankMutation,
                SourceDocumentId = cashBankMutation.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = cashBankMutation.Amount
            };
            debitsourcecashbank = CreateObject(debitsourcecashbank, _accountService);

            journals.Add(credittargetcashbank);
            journals.Add(debitsourcecashbank);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateConfirmationJournalForCashSalesInvoice(CashSalesInvoice cashSalesInvoice, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            DateTime TransactionDate = cashSalesInvoice.ConfirmationDate.GetValueOrDefault(); // ConfirmationDate

            decimal TotalBeforeShippingFee = cashSalesInvoice.Total - cashSalesInvoice.ShippingFee;
            decimal TotalBeforeTax = Math.Round(TotalBeforeShippingFee * 100 / (100 + cashSalesInvoice.Tax));
            decimal TotalBeforeDisc = Math.Round(TotalBeforeTax * 100 / (100 - cashSalesInvoice.Discount));
            decimal Tax = TotalBeforeShippingFee - TotalBeforeTax;
            decimal Disc = TotalBeforeDisc - TotalBeforeTax;

            GeneralLedgerJournal debitcogs = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.COGS).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = (DateTime)cashSalesInvoice.ConfirmationDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = cashSalesInvoice.CoGS //TotalBeforeDisc
            };
            debitcogs = CreateObject(debitcogs, _accountService);

            GeneralLedgerJournal creditinventory = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.TradingGoods).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = (DateTime)cashSalesInvoice.ConfirmationDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = cashSalesInvoice.CoGS, //TotalBeforeDisc
            };
            creditinventory = CreateObject(creditinventory, _accountService);

            //Assuming the items have been delivered
            GeneralLedgerJournal debitreceivable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountReceivableTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = cashSalesInvoice.Total, //Total - cashSalesInvoice.Allowance,
            };
            debitreceivable = CreateObject(debitreceivable, _accountService);

            GeneralLedgerJournal debitdiscount = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.SalesDiscountExpense).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = Disc,
            };
            debitdiscount = CreateObject(debitdiscount, _accountService);

            GeneralLedgerJournal creditrevenuesales = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.SalesRevenue).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = TotalBeforeDisc,
            };
            creditrevenuesales = CreateObject(creditrevenuesales, _accountService);

            GeneralLedgerJournal creditppn = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountPayablePPNkeluaran).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = Tax,
            };
            creditppn = CreateObject(creditppn, _accountService);

            GeneralLedgerJournal creditfreight = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.FreightOut).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = cashSalesInvoice.ShippingFee,
            };
            creditfreight = CreateObject(creditfreight, _accountService);

            journals.Add(debitcogs);
            journals.Add(creditinventory);
            //Assuming the items have been delivered
            journals.Add(debitreceivable);
            journals.Add(debitdiscount);
            journals.Add(creditppn);
            journals.Add(creditfreight);
            journals.Add(creditrevenuesales);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateUnconfirmationJournalForCashSalesInvoice(CashSalesInvoice cashSalesInvoice, DateTime UnconfirmationDate, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            //DateTime UnconfirmationDate = cashSalesInvoice.ConfirmationDate.GetValueOrDefault(); // DateTime.Now;

            decimal TotalBeforeShippingFee = cashSalesInvoice.Total - cashSalesInvoice.ShippingFee;
            decimal TotalBeforeTax = Math.Round(TotalBeforeShippingFee * 100 / (100 + cashSalesInvoice.Tax));
            decimal TotalBeforeDisc = Math.Round(TotalBeforeTax * 100 / (100 - cashSalesInvoice.Discount));
            decimal Tax = TotalBeforeShippingFee - TotalBeforeTax;
            decimal Disc = TotalBeforeDisc - TotalBeforeTax;

            GeneralLedgerJournal creditcogs = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.COGS).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = cashSalesInvoice.CoGS //TotalBeforeDisc
            };
            creditcogs = CreateObject(creditcogs, _accountService);

            GeneralLedgerJournal debitinventory = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.TradingGoods).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = cashSalesInvoice.CoGS, //TotalBeforeDisc
            };
            debitinventory = CreateObject(debitinventory, _accountService);

            //Assuming the items were delivered
            GeneralLedgerJournal creditreceivable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountReceivableTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = cashSalesInvoice.Total, //Total - cashSalesInvoice.Allowance,
            };
            creditreceivable = CreateObject(creditreceivable, _accountService);

            GeneralLedgerJournal debitrevenuesales = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.SalesRevenue).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = TotalBeforeDisc,
            };
            debitrevenuesales = CreateObject(debitrevenuesales, _accountService);

            GeneralLedgerJournal creditdiscount = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.SalesDiscountExpense).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = Disc,
            };
            creditdiscount = CreateObject(creditdiscount, _accountService);

            GeneralLedgerJournal debitppn = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountPayablePPNkeluaran).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = Tax,
            };
            debitppn = CreateObject(debitppn, _accountService);

            GeneralLedgerJournal debitfreight = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.FreightOut).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = cashSalesInvoice.ShippingFee,
            };
            debitfreight = CreateObject(debitfreight, _accountService);

            journals.Add(creditcogs);
            journals.Add(debitinventory);
            //Assuming the items were delivered
            journals.Add(creditreceivable);
            journals.Add(creditdiscount);
            journals.Add(debitppn);
            journals.Add(debitfreight);
            journals.Add(debitrevenuesales);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreatePaidJournalForCashSalesInvoice(CashSalesInvoice cashSalesInvoice, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            DateTime TransactionDate = cashSalesInvoice.PaymentDate.GetValueOrDefault(); // ConfirmationDate

            //decimal TotalBeforeShippingFee = cashSalesInvoice.Total - cashSalesInvoice.ShippingFee;
            //decimal TotalBeforeTax = Math.Round(TotalBeforeShippingFee * 100 / (100 + cashSalesInvoice.Tax));
            //decimal TotalBeforeDisc = Math.Round(TotalBeforeTax * 100 / (100 - cashSalesInvoice.Discount));
            //decimal Tax = TotalBeforeShippingFee - TotalBeforeTax;
            //decimal Disc = TotalBeforeDisc - TotalBeforeTax;

            GeneralLedgerJournal creditreceivable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountReceivableTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = cashSalesInvoice.Allowance,
            };
            creditreceivable = CreateObject(creditreceivable, _accountService);

            GeneralLedgerJournal debitallowance = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.SalesAllowanceExpense).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = cashSalesInvoice.Allowance,
            };
            debitallowance = CreateObject(debitallowance, _accountService);

            journals.Add(creditreceivable);
            journals.Add(debitallowance);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateUnpaidJournalForCashSalesInvoice(CashSalesInvoice cashSalesInvoice, DateTime UnpaidDate, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            //DateTime UnpaidDate = cashSalesInvoice.PaymentDate.GetValueOrDefault(); // DateTime.Now;

            //decimal TotalBeforeShippingFee = cashSalesInvoice.Total - cashSalesInvoice.ShippingFee;
            //decimal TotalBeforeTax = Math.Round(TotalBeforeShippingFee * 100 / (100 + cashSalesInvoice.Tax));
            //decimal TotalBeforeDisc = Math.Round(TotalBeforeTax * 100 / (100 - cashSalesInvoice.Discount));
            //decimal Tax = TotalBeforeShippingFee - TotalBeforeTax;
            //decimal Disc = TotalBeforeDisc - TotalBeforeTax;

            GeneralLedgerJournal debitreceivable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountReceivableTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = UnpaidDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = cashSalesInvoice.Allowance,
            };
            debitreceivable = CreateObject(debitreceivable, _accountService);

            GeneralLedgerJournal creditallowance = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.SalesAllowanceExpense).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesInvoice,
                SourceDocumentId = cashSalesInvoice.Id,
                TransactionDate = UnpaidDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = cashSalesInvoice.Allowance,
            };
            creditallowance = CreateObject(creditallowance, _accountService);

            journals.Add(debitreceivable);
            journals.Add(creditallowance);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateConfirmationJournalForCashSalesReturn(CashSalesReturn cashSalesReturn, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            DateTime TransactionDate = cashSalesReturn.ConfirmationDate.GetValueOrDefault(); // 

            GeneralLedgerJournal creditcogs = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.COGS).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesReturn,
                SourceDocumentId = cashSalesReturn.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = cashSalesReturn.CoGS //Total
            };
            creditcogs = CreateObject(creditcogs, _accountService);

            GeneralLedgerJournal debitinventory = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.TradingGoods).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesReturn,
                SourceDocumentId = cashSalesReturn.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = cashSalesReturn.CoGS, //Total
            };
            debitinventory = CreateObject(debitinventory, _accountService);

            //Assuming the items have arrived
            GeneralLedgerJournal creditpayable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountPayableTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesReturn,
                SourceDocumentId = cashSalesReturn.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = cashSalesReturn.Total, //Total - cashSalesReturn.Allowance,
            };
            creditpayable = CreateObject(creditpayable, _accountService);

            GeneralLedgerJournal debitsalesreturn = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.SalesReturnExpense).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesReturn,
                SourceDocumentId = cashSalesReturn.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = cashSalesReturn.Total,
            };
            debitsalesreturn = CreateObject(debitsalesreturn, _accountService);

            journals.Add(creditcogs);
            journals.Add(debitinventory);
            //Assuming the items have arrived
            journals.Add(creditpayable);
            journals.Add(debitsalesreturn);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateUnconfirmationJournalForCashSalesReturn(CashSalesReturn cashSalesReturn, DateTime UnconfirmationDate, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            //DateTime UnconfirmationDate = cashSalesReturn.ConfirmationDate.GetValueOrDefault(); // DateTime.Now;

            GeneralLedgerJournal debitcogs = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.COGS).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesReturn,
                SourceDocumentId = cashSalesReturn.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = cashSalesReturn.CoGS //Total
            };
            debitcogs = CreateObject(debitcogs, _accountService);

            GeneralLedgerJournal creditinventory = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.TradingGoods).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesReturn,
                SourceDocumentId = cashSalesReturn.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = cashSalesReturn.CoGS, //Total
            };
            creditinventory = CreateObject(creditinventory, _accountService);

            //Assuming the items were arrived
            GeneralLedgerJournal debitpayable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountPayableTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesReturn,
                SourceDocumentId = cashSalesReturn.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = cashSalesReturn.Total, //Total - cashSalesReturn.Allowance,
            };
            debitpayable = CreateObject(debitpayable, _accountService);

            GeneralLedgerJournal creditsalesreturn = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.SalesReturnExpense).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesReturn,
                SourceDocumentId = cashSalesReturn.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = cashSalesReturn.Total,
            };
            creditsalesreturn = CreateObject(creditsalesreturn, _accountService);

            journals.Add(debitcogs);
            journals.Add(creditinventory);
            //Assuming the items were arrived
            journals.Add(debitpayable);
            journals.Add(creditsalesreturn);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreatePaidJournalForCashSalesReturn(CashSalesReturn cashSalesReturn, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            DateTime TransactionDate = cashSalesReturn.PaymentDate.GetValueOrDefault(); // ConfirmationDate

            GeneralLedgerJournal debitpayable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountPayableTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesReturn,
                SourceDocumentId = cashSalesReturn.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = cashSalesReturn.Allowance,
            };
            debitpayable = CreateObject(debitpayable, _accountService);

            GeneralLedgerJournal creditallowance = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.SalesReturnAllowance).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesReturn,
                SourceDocumentId = cashSalesReturn.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Credit,  // contra account
                Amount = cashSalesReturn.Allowance,
            };
            creditallowance = CreateObject(creditallowance, _accountService);

            journals.Add(debitpayable);
            journals.Add(creditallowance);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateUnpaidJournalForCashSalesReturn(CashSalesReturn cashSalesReturn, DateTime UnpaidDate, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            //DateTime UnpaidDate = cashSalesReturn.PaymentDate.GetValueOrDefault(); //ConfirmationDate //DateTime.Now;

            GeneralLedgerJournal creditpayable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountPayableTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesReturn,
                SourceDocumentId = cashSalesReturn.Id,
                TransactionDate = UnpaidDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = cashSalesReturn.Allowance,
            };
            creditpayable = CreateObject(creditpayable, _accountService);

            GeneralLedgerJournal debitallowance = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.SalesReturnAllowance).Id,
                SourceDocument = Constant.GeneralLedgerSource.CashSalesReturn,
                SourceDocumentId = cashSalesReturn.Id,
                TransactionDate = UnpaidDate,
                Status = Constant.GeneralLedgerStatus.Debit,  // contra account
                Amount = cashSalesReturn.Allowance,
            };
            debitallowance = CreateObject(debitallowance, _accountService);

            journals.Add(creditpayable);
            journals.Add(debitallowance);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateConfirmationJournalForStockAdjustment(StockAdjustment stockAdjustment, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();

            if (stockAdjustment.Total >= 0)
            {
                GeneralLedgerJournal debitinventory = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.TradingGoods).Id,
                    SourceDocument = Constant.GeneralLedgerSource.StockAdjustment,
                    SourceDocumentId = stockAdjustment.Id,
                    TransactionDate = (DateTime)stockAdjustment.ConfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Debit,
                    Amount = stockAdjustment.Total
                };
                debitinventory = CreateObject(debitinventory, _accountService);

                GeneralLedgerJournal creditstockequityadjustment = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.EquityAdjustment).Id,
                    SourceDocument = Constant.GeneralLedgerSource.StockAdjustment,
                    SourceDocumentId = stockAdjustment.Id,
                    TransactionDate = (DateTime)stockAdjustment.ConfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Credit,
                    Amount = stockAdjustment.Total
                };
                creditstockequityadjustment = CreateObject(creditstockequityadjustment, _accountService);

                journals.Add(debitinventory);
                journals.Add(creditstockequityadjustment);
            }
            else
            {
                GeneralLedgerJournal debitstockadjustmentexpense = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.StockAdjustmentExpense).Id,
                    SourceDocument = Constant.GeneralLedgerSource.StockAdjustment,
                    SourceDocumentId = stockAdjustment.Id,
                    TransactionDate = (DateTime)stockAdjustment.ConfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Debit,
                    Amount = Math.Abs(stockAdjustment.Total)
                };
                debitstockadjustmentexpense = CreateObject(debitstockadjustmentexpense, _accountService);

                GeneralLedgerJournal creditinventory = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.TradingGoods).Id,
                    SourceDocument = Constant.GeneralLedgerSource.StockAdjustment,
                    SourceDocumentId = stockAdjustment.Id,
                    TransactionDate = (DateTime)stockAdjustment.ConfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Credit,
                    Amount = Math.Abs(stockAdjustment.Total)
                };
                creditinventory = CreateObject(creditinventory, _accountService);

                journals.Add(debitstockadjustmentexpense);
                journals.Add(creditinventory);
            }

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateUnconfirmationJournalForStockAdjustment(StockAdjustment stockAdjustment, DateTime UnconfirmationDate, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            //DateTime UnconfirmationDate = stockAdjustment.ConfirmationDate.GetValueOrDefault(); // DateTime.Now;

            if (stockAdjustment.Total >= 0)
            {
                GeneralLedgerJournal creditinventory = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.TradingGoods).Id,
                    SourceDocument = Constant.GeneralLedgerSource.StockAdjustment,
                    SourceDocumentId = stockAdjustment.Id,
                    TransactionDate = UnconfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Credit,
                    Amount = stockAdjustment.Total
                };
                creditinventory = CreateObject(creditinventory, _accountService);

                GeneralLedgerJournal debitstockequityadjustment = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.EquityAdjustment).Id,
                    SourceDocument = Constant.GeneralLedgerSource.StockAdjustment,
                    SourceDocumentId = stockAdjustment.Id,
                    TransactionDate = UnconfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Debit,
                    Amount = stockAdjustment.Total
                };
                debitstockequityadjustment = CreateObject(debitstockequityadjustment, _accountService);

                journals.Add(creditinventory);
                journals.Add(debitstockequityadjustment);
            }
            else
            {
                GeneralLedgerJournal creditstockadjustmentexpense = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.StockAdjustmentExpense).Id,
                    SourceDocument = Constant.GeneralLedgerSource.StockAdjustment,
                    SourceDocumentId = stockAdjustment.Id,
                    TransactionDate = UnconfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Credit,
                    Amount = Math.Abs(stockAdjustment.Total)
                };
                creditstockadjustmentexpense = CreateObject(creditstockadjustmentexpense, _accountService);

                GeneralLedgerJournal debitinventory = new GeneralLedgerJournal()
                {
                    AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.TradingGoods).Id,
                    SourceDocument = Constant.GeneralLedgerSource.StockAdjustment,
                    SourceDocumentId = stockAdjustment.Id,
                    TransactionDate = UnconfirmationDate,
                    Status = Constant.GeneralLedgerStatus.Debit,
                    Amount = Math.Abs(stockAdjustment.Total)
                };
                debitinventory = CreateObject(debitinventory, _accountService);

                journals.Add(creditstockadjustmentexpense);
                journals.Add(debitinventory);
            }

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateConfirmationJournalForCustomPurchaseInvoice(CustomPurchaseInvoice customPurchaseInvoice, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            DateTime TransactionDate = customPurchaseInvoice.ConfirmationDate.GetValueOrDefault();

            decimal TotalBeforeShippingFee = customPurchaseInvoice.Total - customPurchaseInvoice.ShippingFee;
            decimal TotalBeforeTax = Math.Round(TotalBeforeShippingFee * 100 / (100 + customPurchaseInvoice.Tax));
            decimal TotalBeforeDisc = Math.Round(TotalBeforeTax * 100 / (100 - customPurchaseInvoice.Discount));
            decimal Tax = TotalBeforeShippingFee - TotalBeforeTax;
            decimal Disc = TotalBeforeDisc - TotalBeforeTax;

            GeneralLedgerJournal debitinventory = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.TradingGoods).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = TotalBeforeDisc, //CoGS
            };
            debitinventory = CreateObject(debitinventory, _accountService);

            GeneralLedgerJournal creditgoodsPendingclearance = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.GoodsPendingClearance).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = TotalBeforeDisc, //CoGS
            };
            creditgoodsPendingclearance = CreateObject(creditgoodsPendingclearance, _accountService);

            //Assuming the Goods have arrived
            GeneralLedgerJournal debitGoodsPendingClearance = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.GoodsPendingClearance).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = TotalBeforeDisc, // Total
            };
            debitGoodsPendingClearance = CreateObject(debitGoodsPendingClearance, _accountService);

            GeneralLedgerJournal debitppn = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountReceivablePPNmasukan).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = Tax, // Total
            };
            debitppn = CreateObject(debitppn, _accountService);

            GeneralLedgerJournal debitfreight = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.FreightIn).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = customPurchaseInvoice.ShippingFee,
            };
            debitfreight = CreateObject(debitfreight, _accountService);

            GeneralLedgerJournal creditdiscount = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.PurchaseDiscount).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Credit, // contra account
                Amount = Disc, // Total
            };
            creditdiscount = CreateObject(creditdiscount, _accountService);

            GeneralLedgerJournal creditaccountpayable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountPayableTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = customPurchaseInvoice.Total, //Total - customPurchaseInvoice.Allowance,
            };
            creditaccountpayable = CreateObject(creditaccountpayable, _accountService);

            journals.Add(debitinventory);
            journals.Add(creditgoodsPendingclearance);
            //Assuming the Goods have arrived
            journals.Add(debitGoodsPendingClearance);
            journals.Add(debitppn);
            journals.Add(debitfreight);
            journals.Add(creditdiscount);
            journals.Add(creditaccountpayable);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateUnconfirmationJournalForCustomPurchaseInvoice(CustomPurchaseInvoice customPurchaseInvoice, DateTime UnconfirmationDate, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            //DateTime UnconfirmationDate = customPurchaseInvoice.ConfirmationDate.GetValueOrDefault(); // DateTime.Now;

            decimal TotalBeforeShippingFee = customPurchaseInvoice.Total - customPurchaseInvoice.ShippingFee;
            decimal TotalBeforeTax = Math.Round(TotalBeforeShippingFee * 100 / (100 + customPurchaseInvoice.Tax));
            decimal TotalBeforeDisc = Math.Round(TotalBeforeTax * 100 / (100 - customPurchaseInvoice.Discount));
            decimal Tax = TotalBeforeShippingFee - TotalBeforeTax;
            decimal Disc = TotalBeforeDisc - TotalBeforeTax;

            GeneralLedgerJournal creditinventory = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.TradingGoods).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = TotalBeforeDisc, //CoGS
            };
            creditinventory = CreateObject(creditinventory, _accountService);

            GeneralLedgerJournal debitgoodsPendingclearance = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.GoodsPendingClearance).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = TotalBeforeDisc, //CoGS
            };
            debitgoodsPendingclearance = CreateObject(debitgoodsPendingclearance, _accountService);

            //Assuming the Goods were arrived
            GeneralLedgerJournal creditGoodsPendingClearance = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.GoodsPendingClearance).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = TotalBeforeDisc, // Total
            };
            creditGoodsPendingClearance = CreateObject(creditGoodsPendingClearance, _accountService);

            GeneralLedgerJournal debitaccountpayable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountPayableTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = customPurchaseInvoice.Total, //Total - customPurchaseInvoice.Allowance,
            };
            debitaccountpayable = CreateObject(debitaccountpayable, _accountService);

            GeneralLedgerJournal creditppn = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountReceivablePPNmasukan).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = Tax,
            };
            creditppn = CreateObject(creditppn, _accountService);

            GeneralLedgerJournal creditfreight = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.FreightIn).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Credit,
                Amount = customPurchaseInvoice.ShippingFee,
            };
            creditfreight = CreateObject(creditfreight, _accountService);

            GeneralLedgerJournal debitdisc = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.PurchaseDiscount).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = UnconfirmationDate,
                Status = Constant.GeneralLedgerStatus.Debit, // contra account
                Amount = Disc,
            };
            debitdisc = CreateObject(debitdisc, _accountService);

            journals.Add(creditinventory);
            journals.Add(debitgoodsPendingclearance);
            //Assuming the Goods were arrived
            journals.Add(creditGoodsPendingClearance);
            journals.Add(creditppn);
            journals.Add(creditfreight);
            journals.Add(debitdisc);
            journals.Add(debitaccountpayable);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreatePaidJournalForCustomPurchaseInvoice(CustomPurchaseInvoice customPurchaseInvoice, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            DateTime TransactionDate = customPurchaseInvoice.PaymentDate.GetValueOrDefault(); // ConfirmationDate

            //decimal TotalBeforeShippingFee = customPurchaseInvoice.Total - customPurchaseInvoice.ShippingFee;
            //decimal TotalBeforeTax = Math.Round(TotalBeforeShippingFee * 100 / (100 + customPurchaseInvoice.Tax));
            //decimal TotalBeforeDisc = Math.Round(TotalBeforeTax * 100 / (100 - customPurchaseInvoice.Discount));
            //decimal Tax = TotalBeforeShippingFee - TotalBeforeTax;
            //decimal Disc = TotalBeforeDisc - TotalBeforeTax;

            GeneralLedgerJournal debitaccountpayable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountPayableTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = customPurchaseInvoice.Allowance,
            };
            debitaccountpayable = CreateObject(debitaccountpayable, _accountService);

            GeneralLedgerJournal creditallowance = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.PurchaseAllowance).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = TransactionDate,
                Status = Constant.GeneralLedgerStatus.Credit, // contra account
                Amount = customPurchaseInvoice.Allowance,
            };
            creditallowance = CreateObject(creditallowance, _accountService);
            
            journals.Add(debitaccountpayable);
            journals.Add(creditallowance);

            return journals;
        }

        public IList<GeneralLedgerJournal> CreateUnpaidJournalForCustomPurchaseInvoice(CustomPurchaseInvoice customPurchaseInvoice, DateTime UnpaidDate, IAccountService _accountService)
        {
            IList<GeneralLedgerJournal> journals = new List<GeneralLedgerJournal>();
            //DateTime UnpaidDate = customPurchaseInvoice.PaymentDate.GetValueOrDefault(); // ConfirmationDate.GetValueOrDefault() ?? // DateTime.Now;

            //decimal TotalBeforeShippingFee = customPurchaseInvoice.Total - customPurchaseInvoice.ShippingFee;
            //decimal TotalBeforeTax = Math.Round(TotalBeforeShippingFee * 100 / (100 + customPurchaseInvoice.Tax));
            //decimal TotalBeforeDisc = Math.Round(TotalBeforeTax * 100 / (100 - customPurchaseInvoice.Discount));
            //decimal Tax = TotalBeforeShippingFee - TotalBeforeTax;
            //decimal Disc = TotalBeforeDisc - TotalBeforeTax;

            GeneralLedgerJournal creditaccountpayable = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.AccountPayableTrading).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = UnpaidDate,
                Status = Constant.GeneralLedgerStatus.Debit,
                Amount = customPurchaseInvoice.Allowance,
            };
            creditaccountpayable = CreateObject(creditaccountpayable, _accountService);

            GeneralLedgerJournal debitallowance = new GeneralLedgerJournal()
            {
                AccountId = _accountService.GetObjectByLegacyCode(Constant.AccountLegacyCode.PurchaseAllowance).Id,
                SourceDocument = Constant.GeneralLedgerSource.CustomPurchaseInvoice,
                SourceDocumentId = customPurchaseInvoice.Id,
                TransactionDate = UnpaidDate,
                Status = Constant.GeneralLedgerStatus.Debit, // contra account
                Amount = customPurchaseInvoice.Allowance,
            };
            debitallowance = CreateObject(debitallowance, _accountService);

            journals.Add(creditaccountpayable);
            journals.Add(debitallowance);

            return journals;
        }
    }
}
