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
    public class AccountService : IAccountService
    {
        private IAccountRepository _repository;
        private IAccountValidator _validator;

        public AccountService(IAccountRepository _accountRepository, IAccountValidator _accountValidator)
        {
            _repository = _accountRepository;
            _validator = _accountValidator;
        }

        public IAccountValidator GetValidator()
        {
            return _validator;
        }

        public IQueryable<Account> GetQueryable()
        {
            return _repository.GetQueryable();
        }

        public IList<Account> GetAll()
        {
            return _repository.GetAll();
        }

        public Account GetObjectById(int Id)
        {
            return _repository.GetObjectById(Id);
        }

        public Account GetObjectByIsLegacy(bool IsLegacy)
        {
            return _repository.GetObjectByIsLegacy(IsLegacy);
        }

        public Account CreateObject(Account account, IAccountService _accountService)
        {
            account.Errors = new Dictionary<String, String>();
            return (_validator.ValidCreateObject(account, _accountService) ? _repository.CreateObject(account) : account);
        }

        public Account CreateLegacyObject(Account account, IAccountService _accountService)
        {
            account.IsLegacy = true;
            account.Errors = new Dictionary<String, String>();
            return (_validator.ValidCreateObject(account, _accountService) ? _repository.CreateObject(account) : account);
        }

        public Account SoftDeleteObject(Account account)
        {
            return (_validator.ValidDeleteObject(account) ? _repository.SoftDeleteObject(account) : account);
        }

        public bool DeleteObject(int Id)
        {
            return _repository.DeleteObject(Id);
        }

        
    }
}
