﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Service.Service;
using Core.Interface.Service;
using Core.DomainModel;
using Data.Repository;
using Validation.Validation;
using System.Linq.Dynamic;
using System.Data.Entity;
using Core.Constants;

namespace WebView.Controllers
{
    public class ChartOfAccountController : Controller
    {
        private readonly static log4net.ILog LOG = log4net.LogManager.GetLogger("ChartOfAccountController");
        private IAccountService _accountService;

        public ChartOfAccountController()
        {
            _accountService = new AccountService(new AccountRepository(), new AccountValidator());
        }

        public ActionResult Index()
        {
            if (!AuthenticationModel.IsAllowed("View", Constant.MenuName.Account, Constant.MenuGroupName.Master))
            {
                return Content(Constant.ErrorPage.PageViewNotAllowed);
            }

            return View();
        }

         public dynamic GetList(string _search, long nd, int rows, int? page, string sidx, string sord, string filters = "")
        {
            // Construct where statement
            string strWhere = GeneralFunction.ConstructWhere(filters);
            string filter = null;
            GeneralFunction.ConstructWhereInLinq(strWhere, out filter);
            if (filter == "") filter = "true";
            // Get Data
           var all = _accountService.GetQueryable().Where(x => !x.IsDeleted);
           var parent = _accountService.GetQueryable().Where(x => x.Level < 5 && !x.IsDeleted);
            /*
             * this does not work
           var query = all.LeftOuterJoin(
                           parent,
                           a => a.ParentId,
                           p => p.Id,
                           (account, parentaccount) => new
                           {
                               Id = account.Id,
                               Code = account.Code,
                               Name = account.Name,
                               Group = account.Group,
                               Level = account.Level,
                               ParentId = account.ParentId,
                               ParentCode = parentaccount == null ? "" : parentaccount.Code,
                               Parent = parentaccount == null ? "" : parentaccount.Name,
                               IsLegacy = account.IsLegacy,
                               IsCashBankAccount = account.IsCashBankAccount,
                               LegacyCode = account.LegacyCode
                           }).Where(filter).OrderBy(sidx + " " + sord); //.ToList();
            */
            var query = (from model in all
                         join parentmodel in parent on model.ParentId equals parentmodel.Id into joinedmodel
                         from newaccount in joinedmodel.DefaultIfEmpty()
                         select new
                         {
                             model.Id,
                             model.Code,
                             model.Name,
                             model.Group,
                             model.Level,
                             model.ParentId,
                             ParentCode = newaccount.Code,
                             Parent = newaccount.Name,
                             IsLegacy = model.IsLegacy,
                             IsCashBankAccount = model.IsCashBankAccount,
                             LegacyCode = model.LegacyCode,
                             model.IsLeaf,
                             model.IsUsedBySystem,
                         }).Where(filter).OrderBy(sidx + " " + sord); //.ToList();


            var list = query.AsEnumerable();

            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;
            var totalRecords = query.Count();
            var totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);
            // default last page
            if (totalPages > 0)
            {
                if (!page.HasValue)
                {
                    pageIndex = totalPages - 1;
                    page = totalPages;
                }
            }

            list = list.Skip(pageIndex * pageSize).Take(pageSize);

            return Json(new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = (
                    from model in list
                    select new
                    {
                        id = model.Id,
                        cell = new object[] {
                             model.Id,
                             model.Code,
                             model.Name,
                             model.Group,
                             model.Level,
                             model.ParentId,
                             model.ParentCode,
                             model.Parent,
                             model.IsLegacy,
                             model.IsCashBankAccount,
                             model.LegacyCode,
                             model.IsLeaf,
                             model.IsUsedBySystem,
                      }
                    }).ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

         public dynamic GetLeaves(string _search, long nd, int rows, int? page, string sidx, string sord, string filters = "")
         {
             // Construct where statement
             string strWhere = GeneralFunction.ConstructWhere(filters);
             string filter = null;
             GeneralFunction.ConstructWhereInLinq(strWhere, out filter);
             if (filter == "") filter = "true";
             // Get Data
             var leaves = _accountService.GetQueryable().Where(x => !x.IsDeleted && x.IsLeaf);
             var parent = _accountService.GetQueryable().Where(x => x.Level < 5 && !x.IsDeleted);

             var query = (from model in leaves
                          join parentmodel in parent on model.ParentId equals parentmodel.Id into joinedmodel
                          from newaccount in joinedmodel.DefaultIfEmpty()
                          select new
                          {
                              model.Id,
                              model.Code,
                              model.Name,
                              model.Group,
                              model.Level,
                              model.ParentId,
                              ParentCode = newaccount.Code,
                              Parent = newaccount.Name,
                              IsLegacy = model.IsLegacy,
                              IsCashBankAccount = model.IsCashBankAccount,
                              LegacyCode = model.LegacyCode,
                              model.IsUsedBySystem,
                          }).Where(filter).OrderBy(sidx + " " + sord); //.ToList();


             var list = query.AsEnumerable();

             var pageIndex = Convert.ToInt32(page) - 1;
             var pageSize = rows;
             var totalRecords = query.Count();
             var totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);
             // default last page
             if (totalPages > 0)
             {
                 if (!page.HasValue)
                 {
                     pageIndex = totalPages - 1;
                     page = totalPages;
                 }
             }

             list = list.Skip(pageIndex * pageSize).Take(pageSize);

             return Json(new
             {
                 total = totalPages,
                 page = page,
                 records = totalRecords,
                 rows = (
                     from model in list
                     select new
                     {
                         id = model.Id,
                         cell = new object[] {
                             model.Id,
                             model.Code,
                             model.Name,
                             model.Group,
                             model.Level,
                             model.ParentId,
                             model.ParentCode,
                             model.Parent,
                             model.IsLegacy,
                             model.IsCashBankAccount,
                             model.LegacyCode,
                             model.IsUsedBySystem,
                      }
                     }).ToArray()
             }, JsonRequestBehavior.AllowGet);
         }

        public dynamic Lookup(string _search, long nd, int rows, int? page, string sidx, string sord, int Level, int Group, string filters = "")
        {
             // Construct where statement
             string strWhere = GeneralFunction.ConstructWhere(filters);
             string filter = null;
             GeneralFunction.ConstructWhereInLinq(strWhere, out filter);
             if (filter == "") filter = "true";

             var q = _accountService.GetQueryable().Where(x => x.Level == Level && x.Group == Group && !x.IsDeleted 
                 //&& ((!x.IsLegacy && !x.IsCashBankAccount) || !x.IsLeaf)
                 );

             var query = (from model in q
                          select new
                          {
                              model.Id,
                              model.Code,
                              model.Name,
                              model.Group,
                          }).Where(filter).OrderBy(sidx + " " + sord); //.ToList();

             // Get Data
            var list = query.AsEnumerable();

             var pageIndex = Convert.ToInt32(page) - 1;
             var pageSize = rows;
             var totalRecords = query.Count();
             var totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);
             // default last page
             if (totalPages > 0)
             {
                 if (!page.HasValue)
                 {
                     pageIndex = totalPages - 1;
                     page = totalPages;
                 }
             }

             list = list.Skip(pageIndex * pageSize).Take(pageSize);

             return Json(new
             {
                 total = totalPages,
                 page = page,
                 records = totalRecords,
                 rows = (
                     from model in list
                     select new
                     {
                         id = model.Id,
                         cell = new object[] {
                             model.Id,
                             model.Code,
                             model.Name,
                             model.Group,
                      }
                     }).ToArray()
             }, JsonRequestBehavior.AllowGet);
         }

         public dynamic GetInfo(int Id)
         {
             Account model = new Account();
             try
             {
                 model = _accountService.GetObjectById(Id);
             }
             catch (Exception ex)
             {
                 LOG.Error("GetInfo", ex);
                 Dictionary<string, string> Errors = new Dictionary<string, string>();
                 Errors.Add("Generic", "Error " + ex);

                 return Json(new
                 {
                     Errors
                 }, JsonRequestBehavior.AllowGet);
             }

             return Json(new
             {
                 model.Id,
                 model.Code,
                 model.Name,
                 model.Group,
                 model.Level,
                 model.ParentId,
                 ParentCode = model.ParentId != null ? _accountService.GetObjectById((int)model.ParentId).Code.ToString() : "",
                 Parent = model.ParentId != null ? _accountService.GetObjectById((int)model.ParentId).Name : "",
                 model.IsLegacy,
                 model.IsCashBankAccount,
                 model.LegacyCode,
                 model.IsLeaf,
                 model.IsUsedBySystem,
                 model.Errors
             }, JsonRequestBehavior.AllowGet);
         }

        [HttpPost]
        public dynamic Insert(Account model)
        {
            try
            {
                if (!AuthenticationModel.IsAllowed("Create", Constant.MenuName.Account, Constant.MenuGroupName.Master))
                {
                    Dictionary<string, string> Errors = new Dictionary<string, string>();
                    Errors.Add("Generic", "You are Not Allowed to Add record");

                    return Json(new
                    {
                        Errors
                    }, JsonRequestBehavior.AllowGet);
                }

                model.IsLeaf = true;
                model = _accountService.CreateObject(model);
            }
            catch (Exception ex)
            {
                LOG.Error("Insert Failed", ex);
                Dictionary<string, string> Errors = new Dictionary<string, string>();
                Errors.Add("Generic", "Error " + ex);

                return Json(new
                {
                    Errors
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                model.Errors
            });
        }

        [HttpPost]
        public dynamic Update(Account model)
        {
            try
            {
                if (!AuthenticationModel.IsAllowed("Edit", Core.Constants.Constant.MenuName.Account, Core.Constants.Constant.MenuGroupName.Master))
                {
                    Dictionary<string, string> Errors = new Dictionary<string, string>();
                    Errors.Add("Generic", "You are Not Allowed to Edit Record");

                    return Json(new
                    {
                        Errors
                    }, JsonRequestBehavior.AllowGet);
                }

                var data = _accountService.GetObjectById(model.Id);
                int? oldparentid = data.ParentId;
                data.Name = model.Name;
                if (!data.IsLegacy)
                {
                    data.Code = model.Code;
                    data.Group = model.Group;
                    data.Level = model.Level;
                    data.ParentId = model.ParentId;
                    //data.IsLegacy = model.IsLegacy;
                    //data.IsCashBankAccount = model.IsCashBankAccount;
                    data.LegacyCode = model.LegacyCode;
                    model = _accountService.UpdateObject(data, oldparentid);
                }
                else
                {
                    model = _accountService.UpdateObjectForLegacy(data, oldparentid);
                }
                
            }
            catch (Exception ex)
            {
                LOG.Error("Update Failed", ex);
                model.Errors.Add("Generic", "Error : " + ex);
            }

            return Json(new
            {
                model.Errors,
            });
        }

        [HttpPost]
        public dynamic Delete(Account model)
        {
            try
            {
                if (!AuthenticationModel.IsAllowed("Delete", Core.Constants.Constant.MenuName.Account, Core.Constants.Constant.MenuGroupName.Master))
                {
                    Dictionary<string, string> Errors = new Dictionary<string, string>();
                    Errors.Add("Generic", "You are Not Allowed to Delete Record");

                    return Json(new
                    {
                        Errors
                    }, JsonRequestBehavior.AllowGet);
                }

                var data = _accountService.GetObjectById(model.Id);
                model = _accountService.SoftDeleteObject(data);
            }

            catch (Exception ex)
            {
                LOG.Error("Delete Failed", ex);
                Dictionary<string, string> Errors = new Dictionary<string, string>();
                Errors.Add("Generic", "Error " + ex);

                return Json(new
                {
                    Errors
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                model.Errors
            });
        }
    }
}
