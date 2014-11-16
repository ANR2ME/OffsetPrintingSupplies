﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Core.Interface.Service;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Data.Repository;
using Service.Service;
using Validation.Validation;
using System.Linq.Dynamic;
using System.Data.Entity;
using Core.DomainModel;
using Data.Context;

namespace WebView.Controllers
{
    public class ReportController : Controller
    {
        //
        // GET: /Report/
        private readonly static log4net.ILog LOG = log4net.LogManager.GetLogger("ReportController");
        private IWarehouseItemService _warehouseItemService;
        private IWarehouseService _warehouseService;
        private IItemService _itemService;
        private IItemTypeService _itemTypeService;
        private IUoMService _uoMService;
        private ICompanyService _companyService;
        private ICashBankService _cashBankService;
        private IPayableService _payableService;
        private IPaymentVoucherDetailService _paymentVoucherDetailService;
        private IReceivableService _receivableService;
        private IReceiptVoucherDetailService _receiptVoucherDetailService;
        private ICashSalesInvoiceService _cashSalesInvoiceService;
        private ICashSalesInvoiceDetailService _cashSalesInvoiceDetailService;
        private ICashSalesReturnService _cashSalesReturnService;
        private ICashSalesReturnDetailService _cashSalesReturnDetailService;
        private ICustomPurchaseInvoiceService _customPurchaseInvoiceService;
        private ICustomPurchaseInvoiceDetailService _customPurchaseInvoiceDetailService;

        public class ModelProfitLoss
        {
           public  DateTime StartDate { get; set; }
           public DateTime EndDate { get; set; }
           public decimal TotalSales { get; set; }
           public decimal TotalCoGS { get; set; }
           public decimal TotalSalesReturn { get; set; }
           public decimal TotalExpense { get; set; }
           public string CompanyName { get; set; }
           public string CompanyAddress { get; set; }
           public string CompanyContactNo { get; set; }
        }

        public class ModelFund
        {
            public DateTime FromDueDate { get; set; }
            public DateTime ToDueDate { get; set; }
            public DateTime CurDate { get; set; }
            public decimal cashBank { get; set; }
            public decimal receivable { get; set; }
            public decimal payable { get; set; }
            public decimal dailySalesProjection { get; set; }
            public string CompanyName { get; set; }
            public string CompanyAddress { get; set; }
            public string CompanyContactNo { get; set; }
        }

        public ReportController()
        {
            _warehouseItemService = new WarehouseItemService(new WarehouseItemRepository(), new WarehouseItemValidator());
            _warehouseService = new WarehouseService(new WarehouseRepository(), new WarehouseValidator());
            _itemService = new ItemService(new ItemRepository(), new ItemValidator());
            _itemTypeService = new ItemTypeService(new ItemTypeRepository(), new ItemTypeValidator());
            _uoMService = new UoMService(new UoMRepository(), new UoMValidator());
            _companyService = new CompanyService(new CompanyRepository(), new CompanyValidator());

            _cashBankService = new CashBankService(new CashBankRepository(), new CashBankValidator());
            _payableService = new PayableService(new PayableRepository(), new PayableValidator());
            _paymentVoucherDetailService = new PaymentVoucherDetailService(new PaymentVoucherDetailRepository(), new PaymentVoucherDetailValidator());
            _receivableService = new ReceivableService(new ReceivableRepository(), new ReceivableValidator());
            _receiptVoucherDetailService = new ReceiptVoucherDetailService(new ReceiptVoucherDetailRepository(), new ReceiptVoucherDetailValidator());

            _cashSalesInvoiceService = new CashSalesInvoiceService(new CashSalesInvoiceRepository(), new CashSalesInvoiceValidator());
            _cashSalesInvoiceDetailService = new CashSalesInvoiceDetailService(new CashSalesInvoiceDetailRepository(), new CashSalesInvoiceDetailValidator());
            _cashSalesReturnService = new CashSalesReturnService(new CashSalesReturnRepository(), new CashSalesReturnValidator());
            _cashSalesReturnDetailService = new CashSalesReturnDetailService(new CashSalesReturnDetailRepository(), new CashSalesReturnDetailValidator());
            _customPurchaseInvoiceService = new CustomPurchaseInvoiceService(new CustomPurchaseInvoiceRepository(), new CustomPurchaseInvoiceValidator());
            _customPurchaseInvoiceDetailService = new CustomPurchaseInvoiceDetailService(new CustomPurchaseInvoiceDetailRepository(), new CustomPurchaseInvoiceDetailValidator());
        }

        public ActionResult Item()
        {
            if (!AuthenticationModel.IsAllowed("View", Core.Constants.Constant.MenuName.Item, Core.Constants.Constant.MenuGroupName.Report))
            {
                return Content(Core.Constants.Constant.ErrorPage.PageViewNotAllowed);
            }

            return View();
        }

        public ActionResult ReportItem(int itemTypeId = 0, int warehouseId = 0)
        {
            var company = _companyService.GetQueryable().FirstOrDefault();

            //string filter = "true";
            //if (warehouseId > 0) filter = "WarehouseId == " + warehouseId.ToString();
            //ItemType itemType = _itemTypeService.GetObjectById(itemTypeId);
            //if (itemType != null)
            //{
            //    filter += " && Item.ItemTypeId == " + itemTypeId.ToString();
            //}

            var q = _warehouseItemService.GetQueryable().Include("Warehouse").Include("Item").Include("ItemType").Include("UoM").Where(x => (warehouseId <= 0 || x.WarehouseId == warehouseId) && (itemTypeId <= 0 || x.Item.ItemTypeId == itemTypeId));
            
            var query = (from model in q
                         select new
                         {
                             model.Item.Id,
                             model.Item.Name,
                             model.Item.ItemTypeId,
                             itemtype = model.Item.ItemType.Name,
                             model.Item.Sku,
                             uom = model.Item.UoM.Name,
                             model.Quantity,
                             model.Item.SellingPrice,
                             model.Item.AvgPrice,
                             model.Item.Margin,
                             model.PendingReceival,
                             model.PendingDelivery,
                             CompanyName = company.Name,
                             CompanyAddress = company.Address,
                             CompanyContactNo = company.ContactNo,
                             cogs = model.Item.AvgPrice * model.Quantity,
                             warehouse = model.Warehouse.Code,
                         }).OrderBy(x => x.Sku).ThenBy(y => y.warehouse).ThenBy(z => z.itemtype).ToList();
          
            var rd = new ReportDocument();

            //Loading Report
            rd.Load(Server.MapPath("~/") + "Reports/General/Item.rpt");

            // Setting report data source
            rd.SetDataSource(query);

            var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        public ActionResult ItemDetail()
        {
            if (!AuthenticationModel.IsAllowed("View", Core.Constants.Constant.MenuName.Item, Core.Constants.Constant.MenuGroupName.Report))
            {
                return Content(Core.Constants.Constant.ErrorPage.PageViewNotAllowed);
            }

            return View();
        }

        public ActionResult ReportItemDetail(int warehouseId = 0, DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime? startDay = startDate ?? startDate.GetValueOrDefault().Date;
            DateTime? endDay = endDate ?? endDate.GetValueOrDefault().Date.AddDays(1);
            var db = new OffsetPrintingSuppliesEntities();
            using (db)
            {
                var q = db.WarehouseItems.Include("Warehouse").Include("Item").Include("ItemType").Include("UoM").Where(x => !x.IsDeleted && (warehouseId <= 0 || x.WarehouseId == warehouseId));
                var p = db.CustomPurchaseInvoiceDetails.Include(x => x.Item).Include(x => x.CustomPurchaseInvoice).Where(x => !x.CustomPurchaseInvoice.IsDeleted && x.CustomPurchaseInvoice.IsConfirmed && x.CustomPurchaseInvoice.ConfirmationDate.Value >= startDay/* && x.CustomPurchaseInvoice.ConfirmationDate.Value < endDay*/);
                var s = db.CashSalesInvoiceDetails.Include(x => x.Item).Include(x => x.CashSalesInvoice).Where(x => !x.CashSalesInvoice.IsDeleted && x.CashSalesInvoice.IsConfirmed && x.CashSalesInvoice.ConfirmationDate.Value >= startDay/* && x.CashSalesInvoice.ConfirmationDate.Value < endDay*/);
                var rs = db.CashSalesReturnDetails.Include(x => x.CashSalesInvoiceDetail).Include(x => x.CashSalesReturn).Where(x => !x.CashSalesReturn.IsDeleted && x.CashSalesReturn.IsConfirmed && x.CashSalesReturn.ConfirmationDate.Value >= startDay/* && x.CashSalesReturn.ConfirmationDate.Value < endDay*/);

                var company = _companyService.GetQueryable().FirstOrDefault();

                //string filter = "true";
                //if (warehouseId > 0) filter = "WarehouseId == " + warehouseId.ToString();
                //ItemType itemType = _itemTypeService.GetObjectById(itemTypeId);
                //if (itemType != null)
                //{
                //    filter += " && Item.ItemTypeId == " + itemTypeId.ToString();
                //}

                //var q = _warehouseItemService.GetQueryable().Include("Warehouse").Include("Item").Include("ItemType").Include("UoM").Where(x => (warehouseId <= 0 || x.WarehouseId == warehouseId));

                //var p = _customPurchaseInvoiceDetailService.GetQueryable().Include(x => x.Item).Include(x => x.CustomPurchaseInvoice);
                //var s = _cashSalesInvoiceDetailService.GetQueryable().Include(x => x.Item).Include(x => x.CashSalesInvoice);
                //var rs = _cashSalesReturnDetailService.GetQueryable().Include(x => x.CashSalesInvoiceDetail).Include(x => x.CashSalesReturn);

                var query = (from model in q
                             select new
                             {
                                 model.Item.Id,
                                 model.Item.Name,
                                 model.Item.ItemTypeId,
                                 itemtype = model.Item.ItemType.Name,
                                 model.Item.Sku,
                                 uom = model.Item.UoM.Name,
                                 model.Quantity,
                                 model.Item.SellingPrice,
                                 model.Item.AvgPrice,
                                 model.Item.Margin,
                                 model.PendingReceival,
                                 model.PendingDelivery,
                                 CompanyName = company.Name,
                                 CompanyAddress = company.Address,
                                 CompanyContactNo = company.ContactNo,
                                 cogs = model.Item.AvgPrice * model.Quantity,
                                 warehouse = model.Warehouse.Code,
                                 Purchased = (int?)p.Where(x => x.ItemId == model.ItemId && x.CustomPurchaseInvoice.WarehouseId == model.WarehouseId).Sum(x => x.Quantity) ?? 0,
                                 ReturnedPurchase = 0,
                                 Sold = (int?)s.Where(x => x.ItemId == model.ItemId && x.CashSalesInvoice.WarehouseId == model.WarehouseId).Sum(x => x.Quantity) ?? 0,
                                 ReturnedSales = (int?)rs.Where(x => x.CashSalesInvoiceDetail.ItemId == model.ItemId && x.CashSalesReturn.CashSalesInvoice.WarehouseId == model.WarehouseId).Sum(x => x.Quantity) ?? 0,
                                 StartDate = startDay.Value,
                                 EndDate = DateTime.Today,
                             }).OrderBy(x => x.warehouse).ThenBy(y => y.itemtype).ThenBy(z => z.Sku).ToList();

                var rd = new ReportDocument();

                //Loading Report
                rd.Load(Server.MapPath("~/") + "Reports/General/ItemDetail.rpt");

                // Setting report data source
                rd.SetDataSource(query);

                var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                return File(stream, "application/pdf");
            }
        }

        public ActionResult ProfitLoss()
        {
            if (!AuthenticationModel.IsAllowed("View", Core.Constants.Constant.MenuName.ProfitLoss, Core.Constants.Constant.MenuGroupName.Report))
            {
                return Content(Core.Constants.Constant.ErrorPage.PageViewNotAllowed);
            }

            return View();
        }

        public ActionResult ReportProfitLoss(DateTime startDate, DateTime endDate)
        {
            var company = _companyService.GetQueryable().FirstOrDefault();
            DateTime endDay = endDate.Date.AddDays(1);
            var cashSalesReturnPayables = _payableService.GetQueryable().Where(x => x.IsCompleted &&
                            x.CompletionDate.Value >= startDate && x.CompletionDate.Value < endDay && x.PayableSource == Core.Constants.Constant.PayableSource.CashSalesReturn);
            decimal totalSalesReturnPayable = 0;
            foreach (var payable in cashSalesReturnPayables)
            {
                totalSalesReturnPayable += (payable.Amount - payable.AllowanceAmount);
            }
            
            var paymentRequestPayables = _payableService.GetQueryable().Where(x => x.IsCompleted &&
                            x.CompletionDate.Value >= startDate && x.CompletionDate.Value < endDay && x.PayableSource == Core.Constants.Constant.PayableSource.PaymentRequest);
            decimal totalPaymentRequestPayable = 0;
            foreach (var payable in cashSalesReturnPayables)
            {
                totalPaymentRequestPayable += (payable.Amount - payable.AllowanceAmount);
            }

            var cashSalesInvoices = _cashSalesInvoiceService.GetQueryable().Where(x => x.IsConfirmed &&
                            x.ConfirmationDate.Value >= startDate && x.ConfirmationDate.Value < endDay).ToList();
            decimal totalCashSales = 0;
            decimal totalCoGS = 0;
            foreach (var cashSales in cashSalesInvoices)
            {
                totalCashSales += (cashSales.Total - cashSales.Allowance);
                totalCoGS += cashSales.CoGS;
            }

            List<ModelProfitLoss> query = new List<ModelProfitLoss>();
            var profitloss = new ModelProfitLoss
                         {
                             StartDate = startDate.Date,
                             EndDate = endDate.Date,
                             TotalSales = totalCashSales,
                             TotalCoGS = totalCoGS,
                             TotalSalesReturn = totalSalesReturnPayable,
                             TotalExpense = totalPaymentRequestPayable,
                             CompanyName = company.Name,
                             CompanyAddress = company.Address,
                             CompanyContactNo = company.ContactNo,
                         };

            query.Add(profitloss);

            var rd = new ReportDocument();

            //Loading Report
            rd.Load(Server.MapPath("~/") + "Reports/General/ProfitLoss.rpt");

            // Setting report data source
            rd.SetDataSource(query);

            var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        public ActionResult Purchase()
        {
            if (!AuthenticationModel.IsAllowed("View", Core.Constants.Constant.MenuName.Purchase, Core.Constants.Constant.MenuGroupName.Report))
            {
                return Content(Core.Constants.Constant.ErrorPage.PageViewNotAllowed);
            }

            return View();
        }

        public ActionResult ReportPurchase(DateTime startDate, DateTime endDate)
        {
            var company = _companyService.GetQueryable().FirstOrDefault();
            DateTime endDay = endDate.Date.AddDays(1);
            var q = _customPurchaseInvoiceService.GetQueryable().Include("CashBank").Include("Warehouse").Where(x => x.IsConfirmed && x.ConfirmationDate.Value >= startDate && x.ConfirmationDate.Value < endDay)
                                                .OrderBy(x => x.ConfirmationDate.Value)
                                                .ThenBy(x => x.Id);

            var query = (from model in q
                         select new
                         {
                             StartDate = startDate,
                             EndDate = endDate,
                             model.Code,
                             model.Description,
                             model.PurchaseDate,
                             ConfirmationDate = model.ConfirmationDate.Value,
                             DueDate = model.DueDate.Value,
                             model.Discount,
                             model.Tax,
                             model.Allowance,
                             model.CoGS,
                             AmountPaid = model.AmountPaid ?? 0,
                             model.Total,
                             CashBank = model.CashBank.Name,
                             Warehouse = model.Warehouse.Name,
                             CompanyName = company.Name,
                             CompanyAddress = company.Address,
                             CompanyContactNo = company.ContactNo
                         }).ToList();

            if (!query.Any()) return Content(Core.Constants.Constant.ErrorPage.RecordNotFound);

            var rd = new ReportDocument();

            //Loading Report
            rd.Load(Server.MapPath("~/") + "Reports/General/Purchase.rpt");

            // Setting report data source
            rd.SetDataSource(query);

            var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        public ActionResult PurchaseDetail()
        {
            if (!AuthenticationModel.IsAllowed("View", Core.Constants.Constant.MenuName.Purchase, Core.Constants.Constant.MenuGroupName.Report))
            {
                return Content(Core.Constants.Constant.ErrorPage.PageViewNotAllowed);
            }

            return View();
        }

        public ActionResult ReportPurchaseDetail(DateTime startDate, DateTime endDate)
        {
            var company = _companyService.GetQueryable().FirstOrDefault();
            DateTime endDay = endDate.Date.AddDays(1);
            var q = _customPurchaseInvoiceDetailService.GetQueryable().Include("Item").Include("UoM").Include("PriceMutation").Include("CashInvoiceSales").Include("CashBank").Include("Warehouse").Where(x => x.CustomPurchaseInvoice.IsConfirmed && x.CustomPurchaseInvoice.ConfirmationDate.Value >= startDate && x.CustomPurchaseInvoice.ConfirmationDate.Value < endDay)
                                                    .OrderBy(x => x.CustomPurchaseInvoice.ConfirmationDate.Value)
                                                    .ThenBy(x => x.CustomPurchaseInvoiceId);

            var query = (from model in q
                         select new
                         {
                             StartDate = startDate,
                             EndDate = endDate,
                             model.CustomPurchaseInvoice.Code,
                             model.CustomPurchaseInvoice.Description,
                             model.CustomPurchaseInvoice.PurchaseDate,
                             ConfirmationDate = model.CustomPurchaseInvoice.ConfirmationDate.Value,
                             DueDate = model.CustomPurchaseInvoice.DueDate.Value,
                             model.CustomPurchaseInvoice.Discount,
                             model.CustomPurchaseInvoice.Tax,
                             model.CustomPurchaseInvoice.Allowance,
                             model.CustomPurchaseInvoice.CoGS,
                             AmountPaid = model.CustomPurchaseInvoice.AmountPaid ?? 0,
                             model.CustomPurchaseInvoice.Total,
                             CashBank = model.CustomPurchaseInvoice.CashBank.Name,
                             Warehouse = model.CustomPurchaseInvoice.Warehouse.Name,
                             CompanyName = company.Name,
                             CompanyAddress = company.Address,
                             CompanyContactNo = company.ContactNo,
                             SKU = model.Item.Sku,
                             ItemName = model.Item.Name,
                             Quantity = model.Quantity,
                             UoM = model.Item.UoM.Name,
                             ItemDisc = model.Discount,
                             ItemPrice = model.ListedUnitPrice,
                         }).ToList();

            if (!query.Any()) return Content(Core.Constants.Constant.ErrorPage.RecordNotFound);

            var rd = new ReportDocument();

            //Loading Report
            rd.Load(Server.MapPath("~/") + "Reports/General/PurchaseDetail.rpt");

            // Setting report data source
            rd.SetDataSource(query);

            var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        public ActionResult Sales()
        {
            if (!AuthenticationModel.IsAllowed("View", Core.Constants.Constant.MenuName.Sales, Core.Constants.Constant.MenuGroupName.Report))
            {
                return Content(Core.Constants.Constant.ErrorPage.PageViewNotAllowed);
            }

            return View();
        }

        public ActionResult ReportSales(DateTime startDate, DateTime endDate)
        {
            var company = _companyService.GetQueryable().FirstOrDefault();
            DateTime endDay = endDate.Date.AddDays(1);
            var q = _cashSalesInvoiceService.GetQueryable().Include("CashBank").Include("Warehouse").Where(x => x.IsConfirmed && x.ConfirmationDate.Value >= startDate && x.ConfirmationDate.Value < endDay)
                                                .OrderBy(x => x.ConfirmationDate.Value)
                                                .ThenBy(x => x.Id);

            var query = (from model in q
                         select new
                         {
                             StartDate = startDate,
                             EndDate = endDate,
                             model.Code,
                             model.Description,
                             model.SalesDate,
                             ConfirmationDate = model.ConfirmationDate.Value,
                             DueDate = model.DueDate.Value,
                             model.Discount,
                             model.Tax,
                             model.Allowance,
                             model.CoGS,
                             AmountPaid = model.AmountPaid.Value,
                             model.Total,
                             CashBank = model.CashBank.Name,
                             Warehouse = model.Warehouse.Name,
                             CompanyName = company.Name,
                             CompanyAddress = company.Address,
                             CompanyContactNo = company.ContactNo
                         }).ToList();

            if (!query.Any()) return Content(Core.Constants.Constant.ErrorPage.RecordNotFound);

            var rd = new ReportDocument();

            //Loading Report
            rd.Load(Server.MapPath("~/") + "Reports/General/Sales.rpt");

            // Setting report data source
            rd.SetDataSource(query);

            var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        public ActionResult SalesDetail()
        {
            if (!AuthenticationModel.IsAllowed("View", Core.Constants.Constant.MenuName.Sales, Core.Constants.Constant.MenuGroupName.Report))
            {
                return Content(Core.Constants.Constant.ErrorPage.PageViewNotAllowed);
            }

            return View();
        }

        public ActionResult ReportSalesDetail(DateTime startDate, DateTime endDate)
        {
            var company = _companyService.GetQueryable().FirstOrDefault();
            DateTime endDay = endDate.Date.AddDays(1);
            var q = _cashSalesInvoiceDetailService.GetQueryable().Include("Item").Include("UoM").Include("PriceMutation").Include("CashInvoiceSales").Include("CashBank").Include("Warehouse").Where(x => x.CashSalesInvoice.IsConfirmed && x.CashSalesInvoice.ConfirmationDate.Value >= startDate && x.CashSalesInvoice.ConfirmationDate.Value < endDay)
                                                    .OrderBy(x => x.CashSalesInvoice.ConfirmationDate.Value)
                                                    .ThenBy(x => x.CashSalesInvoiceId);

            var query = (from model in q
                         select new
                         {
                             StartDate = startDate,
                             EndDate = endDate,
                             model.CashSalesInvoice.Code,
                             model.CashSalesInvoice.Description,
                             model.CashSalesInvoice.SalesDate,
                             ConfirmationDate = model.CashSalesInvoice.ConfirmationDate.Value,
                             DueDate = model.CashSalesInvoice.DueDate.Value,
                             model.CashSalesInvoice.Discount,
                             model.CashSalesInvoice.Tax,
                             model.CashSalesInvoice.Allowance,
                             model.CashSalesInvoice.CoGS,
                             AmountPaid = model.CashSalesInvoice.AmountPaid.Value,
                             model.CashSalesInvoice.Total,
                             CashBank = model.CashSalesInvoice.CashBank.Name,
                             Warehouse = model.CashSalesInvoice.Warehouse.Name,
                             CompanyName = company.Name,
                             CompanyAddress = company.Address,
                             CompanyContactNo = company.ContactNo,
                             SKU = model.Item.Sku,
                             ItemName = model.Item.Name,
                             Quantity = model.Quantity,
                             UoM = model.Item.UoM.Name,
                             ItemDisc = model.Discount,
                             ItemPrice = (model.IsManualPriceAssignment? model.AssignedPrice : model.PriceMutation.Amount),
                         }).ToList();

            if (!query.Any()) return Content(Core.Constants.Constant.ErrorPage.RecordNotFound);

            var rd = new ReportDocument();

            //Loading Report
            rd.Load(Server.MapPath("~/") + "Reports/General/SalesDetail.rpt");

            // Setting report data source
            rd.SetDataSource(query);

            var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        public ActionResult TopSales()
        {
            if (!AuthenticationModel.IsAllowed("View", Core.Constants.Constant.MenuName.TopSales, Core.Constants.Constant.MenuGroupName.Report))
            {
                return Content(Core.Constants.Constant.ErrorPage.PageViewNotAllowed);
            }

            return View();
        }

        public ActionResult ReportTopSales(DateTime startDate, DateTime endDate, int maxItem)
        {
            var company = _companyService.GetQueryable().FirstOrDefault();
            DateTime endDay = endDate.Date.AddDays(1);
            var q = _cashSalesInvoiceDetailService.GetQueryable().Include("CashSalesInvoice").Include("Item").Include("UoM").Where(x => x.CashSalesInvoice.IsConfirmed && x.CashSalesInvoice.ConfirmationDate.Value >= startDate && x.CashSalesInvoice.ConfirmationDate.Value < endDay);

            if (maxItem <= 0 || !q.Any()) return Content(Core.Constants.Constant.ErrorPage.RecordNotFound);

            var query = (from m in q
                         group m by m.ItemId into g
                         orderby g.Sum(x => x.Amount - x.CoGS) descending
                         select new
                         {
                             StartDate = startDate,
                             EndDate = endDate,
                             CompanyName = company.Name,
                             CompanyAddress = company.Address,
                             CompanyContactNo = company.ContactNo,
                             SKU = g.FirstOrDefault().Item.Sku,
                             Name = g.FirstOrDefault().Item.Name,
                             Quantity = g.Sum(x => x.Quantity),
                             uom = g.FirstOrDefault().Item.UoM.Name,
                             CoGS = g.Sum(x => x.CoGS), 
                             Amount = g.Sum(x => x.Amount),
                             profitloss = g.Sum(x => x.Amount - x.CoGS)
                         }).Take(maxItem).ToList();

            var rd = new ReportDocument();

            //Loading Report
            rd.Load(Server.MapPath("~/") + "Reports/General/TopSales.rpt");

            // Setting report data source
            rd.SetDataSource(query);

            var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        public ActionResult Funds()
        {
            return View();
        }

        public ActionResult ReportFunds(int Id, DateTime DueDate, decimal Discount, decimal Tax, decimal ShippingFee, decimal Allowance, decimal DailySalesProjection, bool IncludeSaturdaySales, bool IncludeSundaySales)
        {
            var company = _companyService.GetQueryable().FirstOrDefault();
            DateTime startDate = DateTime.Today;
            DateTime endDate = DueDate.Date;

            var data = _customPurchaseInvoiceService.GetObjectById(Id);
            data.DueDate = DueDate;
            data.Discount = Discount;
            data.Tax = Tax;
            data.ShippingFee = ShippingFee;
            data.Allowance = Allowance;
            decimal total = _customPurchaseInvoiceService.CalculateTotalAmountAfterDiscountAndTax(data) - Allowance;
            decimal totalcashbank = _cashBankService.GetTotalCashBank();

            DateTime curDate = startDate;
            decimal funds = totalcashbank;
            var query = new List<ModelFund>();
            while (curDate <= endDate)
            {
                decimal receivable = _receivableService.GetTotalRemainingAmountByDueDate(curDate, curDate);
                decimal payable = _payableService.GetTotalRemainingAmountByDueDate(curDate, curDate);
                decimal sales = 0;
                if ((curDate.DayOfWeek != DayOfWeek.Saturday && curDate.DayOfWeek != DayOfWeek.Sunday) ||
                    (curDate.DayOfWeek == DayOfWeek.Saturday && IncludeSaturdaySales) ||
                    (curDate.DayOfWeek == DayOfWeek.Sunday && IncludeSundaySales))
                {
                    sales = DailySalesProjection;
                }

                var curFund = new ModelFund()
                {
                    FromDueDate = startDate,
                    ToDueDate = endDate,
                    CurDate = curDate,
                    cashBank = funds,
                    payable = payable,
                    receivable = receivable,
                    dailySalesProjection = sales,
                    CompanyName = company.Name,
                    CompanyAddress = company.Address,
                    CompanyContactNo = company.ContactNo
                };
                query.Add(curFund);

                funds += sales;
                funds += receivable;
                funds -= payable;

                curDate = curDate.AddDays(1);
            }

            var rd = new ReportDocument();

            //Loading Report
            rd.Load(Server.MapPath("~/") + "Reports/General/Funds.rpt");

            // Setting report data source
            rd.SetDataSource(query);

            var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);

            //var response = Request.CreateResponse(HttpStatusCode.OK);
            //response.Headers.Clear();
            //response.Content = new StreamContent(pdf);
            //response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            //return response;
            
            return File(stream, "application/pdf");
        }

        public ActionResult PurchaseInvoice()
        {
            return View();
        }

        public ActionResult ReportPurchaseInvoice(int Id)
        {
            var company = _companyService.GetQueryable().FirstOrDefault();
            var q = _customPurchaseInvoiceDetailService.GetQueryableObjectsByCustomPurchaseInvoiceId(Id).Include("CustomPurchaseInvoice").Include("Item").Include("UoM");
            string user = AuthenticationModel.GetUserName();

            var query = (from model in q
                         select new
                         {
                             SKU = model.Item.Sku,
                             Name = model.Item.Name,
                             UoM = model.Item.UoM.Name,
                             model.Quantity,
                             Price = model.ListedUnitPrice,
                             model.Discount,
                             GlobalDiscount = model.CustomPurchaseInvoice.Discount,
                             Tax = model.CustomPurchaseInvoice.Tax,
                             ShippingFee = model.CustomPurchaseInvoice.ShippingFee,
                             Allowance = model.CustomPurchaseInvoice.Allowance,
                             Code = model.CustomPurchaseInvoice.Code,
                             Date = model.CustomPurchaseInvoice.ConfirmationDate.Value,
                             contact = "", // model.CustomPurchaseInvoice.Contact.Name,
                             CompanyName = company.Name,
                             CompanyAddress = company.Address,
                             CompanyContactNo = company.ContactNo,
                             User = user,
                             Description = model.CustomPurchaseInvoice.Description,
                         }).ToList();

            if (!query.Any()) return Content(Core.Constants.Constant.ErrorPage.RecordDetailNotFound);

            var rd = new ReportDocument();

            //Loading Report
            rd.Load(Server.MapPath("~/") + "Reports/General/PurchaseInvoice.rpt");

            // Setting report data source
            rd.SetDataSource(query);

            var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        public ActionResult PaymentVoucher()
        {
            return View();
        }

        public ActionResult ReportPaymentVoucher(int Id)
        {
            var company = _companyService.GetQueryable().FirstOrDefault();
            //var paymentVoucher = _paymentVoucherService.GetObjectById(Id);
            var q = _paymentVoucherDetailService.GetQueryableObjectsByPaymentVoucherId(Id).Include("PaymentVoucher").Include("Payable");
            string user = AuthenticationModel.GetUserName();

            var query = (from model in q
                         select new
                         {
                             Code = model.PaymentVoucher.Code,
                             Date = (model.PaymentVoucher.ConfirmationDate != null) ? model.PaymentVoucher.ConfirmationDate.Value : DateTime.Today,
                             SourceCode = model.Payable.PayableSourceCode,
                             Source = model.Payable.PayableSource,
                             Amount = model.Amount,
                             Description = model.Description,
                             contact = "",
                             CompanyName = company.Name,
                             CompanyAddress = company.Address,
                             CompanyContactNo = company.ContactNo,
                             User = user,
                         }).ToList();

            if (!query.Any()) return Content(Core.Constants.Constant.ErrorPage.RecordDetailNotFound);

            var rd = new ReportDocument();

            //Loading Report
            rd.Load(Server.MapPath("~/") + "Reports/General/PaymentVoucher.rpt");

            // Setting report data source
            rd.SetDataSource(query);

            // Set printer paper size
            System.Drawing.Printing.PrintDocument doctoprint = new System.Drawing.Printing.PrintDocument();
            doctoprint.PrinterSettings.PrinterName = "Microsoft XPS Document Writer";
            int i = 0;
            for (i = 0; i < doctoprint.PrinterSettings.PaperSizes.Count; i++)
            {
                int rawKind = 0;
                if (doctoprint.PrinterSettings.PaperSizes[i].PaperName.ToLower() == "struk")
                {
                    rawKind = Convert.ToInt32(doctoprint.PrinterSettings.PaperSizes[i].GetType().GetField("kind", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(doctoprint.PrinterSettings.PaperSizes[i]));
                    if (Enum.IsDefined(typeof(CrystalDecisions.Shared.PaperSize), rawKind))
                    {
                        rd.PrintOptions.PaperSize = (CrystalDecisions.Shared.PaperSize)rawKind;
                        //rd.PrintOptions.PaperOrientation = PaperOrientation.Landscape;
                    }
                    break;
                }
            }

            var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        public ActionResult ReceiptVoucher()
        {
            return View();
        }

        public ActionResult ReportReceiptVoucher(int Id)
        {
            var company = _companyService.GetQueryable().FirstOrDefault();
            //var receiptVoucher = _receiptVoucherService.GetObjectById(Id);
            var q = _receiptVoucherDetailService.GetQueryableObjectsByReceiptVoucherId(Id).Include("ReceiptVoucher").Include("Receivable");
            string user = AuthenticationModel.GetUserName();

            var query = (from model in q
                         select new
                         {
                             Code = model.ReceiptVoucher.Code,
                             Date = (model.ReceiptVoucher.ConfirmationDate != null) ? model.ReceiptVoucher.ConfirmationDate.Value : DateTime.Today,
                             SourceCode = model.Receivable.ReceivableSourceCode,
                             Source = model.Receivable.ReceivableSource,
                             Amount = model.Amount,
                             Description = model.Description,
                             contact = "",
                             CompanyName = company.Name,
                             CompanyAddress = company.Address,
                             CompanyContactNo = company.ContactNo,
                             User = user,
                         }).ToList();

            if (!query.Any()) return Content(Core.Constants.Constant.ErrorPage.RecordDetailNotFound);

            var rd = new ReportDocument();

            //Loading Report
            rd.Load(Server.MapPath("~/") + "Reports/General/ReceiptVoucher.rpt");

            // Setting report data source
            rd.SetDataSource(query);

            // Set printer paper size
            System.Drawing.Printing.PrintDocument doctoprint = new System.Drawing.Printing.PrintDocument();
            doctoprint.PrinterSettings.PrinterName = "Microsoft XPS Document Writer";
            int i = 0;
            for (i = 0; i < doctoprint.PrinterSettings.PaperSizes.Count; i++)
            {
                int rawKind = 0;
                if (doctoprint.PrinterSettings.PaperSizes[i].PaperName.ToLower() == "struk")
                {
                    rawKind = Convert.ToInt32(doctoprint.PrinterSettings.PaperSizes[i].GetType().GetField("kind", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(doctoprint.PrinterSettings.PaperSizes[i]));
                    if (Enum.IsDefined(typeof(CrystalDecisions.Shared.PaperSize), rawKind))
                    {
                        rd.PrintOptions.PaperSize = (CrystalDecisions.Shared.PaperSize)rawKind;
                        //rd.PrintOptions.PaperOrientation = PaperOrientation.Landscape;
                    }
                    break;
                }
            }

            var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        public ActionResult SalesInvoice()
        {
            return View();
        }

        public ActionResult ReportSalesInvoice(int Id)
        {
            var company = _companyService.GetQueryable().FirstOrDefault();
            //var cashSalesInvoice = _cashSalesInvoiceService.GetObjectById(Id);
            var q = _cashSalesInvoiceDetailService.GetQueryableObjectsByCashSalesInvoiceId(Id).Include("CashSalesInvoice").Include("Item").Include("UoM");
            string user = AuthenticationModel.GetUserName();

            var query = (from model in q
                         select new
                         {
                             SKU = model.Item.Sku,
                             Name = model.Item.Name,
                             UoM = model.Item.UoM.Name,
                             model.Quantity,
                             Price = model.IsManualPriceAssignment? model.AssignedPrice : model.Item.SellingPrice,
                             model.Discount,
                             GlobalDiscount = model.CashSalesInvoice.Discount,
                             ShippingFee = model.CashSalesInvoice.ShippingFee,
                             Tax = model.CashSalesInvoice.Tax,
                             Allowance = model.CashSalesInvoice.Allowance,
                             Code = model.CashSalesInvoice.Code,
                             Date = (model.CashSalesInvoice.ConfirmationDate != null) ? model.CashSalesInvoice.ConfirmationDate.Value : DateTime.Today,
                             contact = model.CashSalesInvoice.ContactName,
                             contactPhone = model.CashSalesInvoice.ContactPhone,
                             CompanyName = company.Name,
                             CompanyAddress = company.Address,
                             CompanyContactNo = company.ContactNo,
                             User = user,
                             Description = model.CashSalesInvoice.Description,
                         }).ToList();

            if (!query.Any()) return Content(Core.Constants.Constant.ErrorPage.RecordDetailNotFound);

            var rd = new ReportDocument();

            //Loading Report
            rd.Load(Server.MapPath("~/") + "Reports/General/SalesInvoice.rpt");

            // Setting report data source
            rd.SetDataSource(query);

            // Set printer paper size
            System.Drawing.Printing.PrintDocument doctoprint = new System.Drawing.Printing.PrintDocument();
            doctoprint.PrinterSettings.PrinterName = "Microsoft XPS Document Writer";
            int i = 0;
            for (i = 0; i < doctoprint.PrinterSettings.PaperSizes.Count; i++)
            {
                int rawKind = 0;
                if (doctoprint.PrinterSettings.PaperSizes[i].PaperName.ToLower() == "struk")
                {
                    rawKind = Convert.ToInt32(doctoprint.PrinterSettings.PaperSizes[i].GetType().GetField("kind", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(doctoprint.PrinterSettings.PaperSizes[i]));
                    if (Enum.IsDefined(typeof(CrystalDecisions.Shared.PaperSize), rawKind))
                    {
                        rd.PrintOptions.PaperSize = (CrystalDecisions.Shared.PaperSize)rawKind;
                        //rd.PrintOptions.PaperOrientation = PaperOrientation.Landscape;
                    }
                    break;
                }
            }

            var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        public ActionResult ReportSalesInvoiceForCustomer(int Id)
        {
            var company = _companyService.GetQueryable().FirstOrDefault();
            //var cashSalesInvoice = _cashSalesInvoiceService.GetObjectById(Id);
            var q = _cashSalesInvoiceDetailService.GetQueryableObjectsByCashSalesInvoiceId(Id).Include("CashSalesInvoice").Include("Item").Include("UoM");
            string user = AuthenticationModel.GetUserName();

            var query = (from model in q
                         select new
                         {
                             SKU = model.Item.Sku,
                             Name = model.Item.Name,
                             UoM = model.Item.UoM.Name,
                             model.Quantity,
                             Price = model.IsManualPriceAssignment ? model.AssignedPrice : model.Item.SellingPrice,
                             Discount = model.Discount,
                             GlobalDiscount = model.CashSalesInvoice.Discount,
                             Tax = model.CashSalesInvoice.Tax,
                             ShippingFee = model.CashSalesInvoice.ShippingFee,
                             Allowance = 0, //model.CashSalesInvoice.Allowance,
                             Code = model.CashSalesInvoice.Code,
                             Date = (model.CashSalesInvoice.ConfirmationDate != null) ? model.CashSalesInvoice.ConfirmationDate.Value : DateTime.Today,
                             contact = model.CashSalesInvoice.ContactName,
                             contactPhone = model.CashSalesInvoice.ContactPhone,
                             CompanyName = company.Name,
                             CompanyAddress = company.Address,
                             CompanyContactNo = company.ContactNo,
                             User = user,
                             Description = model.CashSalesInvoice.Description,
                         }).ToList();

            if (!query.Any()) return Content(Core.Constants.Constant.ErrorPage.RecordDetailNotFound);

            var rd = new ReportDocument();

            //Loading Report
            rd.Load(Server.MapPath("~/") + "Reports/General/SalesInvoice.rpt");

            // Setting report data source
            rd.SetDataSource(query);

            // Set printer paper size
            System.Drawing.Printing.PrintDocument doctoprint = new System.Drawing.Printing.PrintDocument();
            doctoprint.PrinterSettings.PrinterName = "Microsoft XPS Document Writer";
            int i = 0;
            for (i = 0; i < doctoprint.PrinterSettings.PaperSizes.Count; i++)
            {
                int rawKind = 0;
                if (doctoprint.PrinterSettings.PaperSizes[i].PaperName.ToLower() == "struk")
                {
                    rawKind = Convert.ToInt32(doctoprint.PrinterSettings.PaperSizes[i].GetType().GetField("kind", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(doctoprint.PrinterSettings.PaperSizes[i]));
                    if (Enum.IsDefined(typeof(CrystalDecisions.Shared.PaperSize), rawKind))
                    {
                        rd.PrintOptions.PaperSize = (CrystalDecisions.Shared.PaperSize)rawKind;
                        //rd.PrintOptions.PaperOrientation = PaperOrientation.Landscape;
                    }
                    break;
                }
            }

            var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        public ActionResult SalesReturnInvoice()
        {
            return View();
        }

        public ActionResult ReportSalesReturnInvoice(int Id)
        {
            var company = _companyService.GetQueryable().FirstOrDefault();
            //var cashSalesReturnInvoice = _cashSalesInvoiceService.GetObjectById(Id);
            var q = _cashSalesReturnDetailService.GetQueryableObjectsByCashSalesReturnId(Id).Include("CashSalesReturn").Include("CashSalesInvoiceDetail").Include("CashSalesInvoice").Include("Item").Include("UoM");
            string user = AuthenticationModel.GetUserName();

            var query = (from model in q
                         select new
                         {
                             SKU = model.CashSalesInvoiceDetail.Item.Sku,
                             Name = model.CashSalesInvoiceDetail.Item.Name,
                             UoM = model.CashSalesInvoiceDetail.Item.UoM.Name,
                             Quantity = model.Quantity,
                             Price = model.CashSalesInvoiceDetail.IsManualPriceAssignment ? model.CashSalesInvoiceDetail.AssignedPrice : model.CashSalesInvoiceDetail.Item.SellingPrice,
                             Amount = model,
                             Discount = model.CashSalesInvoiceDetail.Discount,
                             GlobalDiscount = model.CashSalesInvoiceDetail.CashSalesInvoice.Discount,
                             Tax = model.CashSalesInvoiceDetail.CashSalesInvoice.Tax,
                             Allowance = model.CashSalesReturn.Allowance,
                             Code = model.CashSalesReturn.Code,
                             SalesCode = model.CashSalesInvoiceDetail.CashSalesInvoice.Code,
                             Date = (model.CashSalesReturn.ConfirmationDate != null) ? model.CashSalesReturn.ConfirmationDate.Value : model.CashSalesReturn.ReturnDate.Value,
                             contact = "",
                             CompanyName = company.Name,
                             CompanyAddress = company.Address,
                             CompanyContactNo = company.ContactNo,
                             User = user,
                             Description = model.CashSalesReturn.Description,
                         }).ToList();

            var rd = new ReportDocument();

            //Loading Report
            rd.Load(Server.MapPath("~/") + "Reports/General/SalesReturnInvoice.rpt");

            // Setting report data source
            rd.SetDataSource(query);

            var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

    }
}
