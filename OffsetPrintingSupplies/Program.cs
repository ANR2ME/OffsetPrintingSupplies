﻿using Core.DomainModel;
using Core.Interface.Service;
using Data.Context;
using Data.Repository;
using Service.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestValidation;
using Validation.Validation;

namespace OffsetPrintingSupplies
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var db = new OffsetPrintingSuppliesEntities();
            using (db)
            {
                db.DeleteAllTables();

                DataBuilder d = new DataBuilder();
                //PurchaseBuilder p = new PurchaseBuilder();
                //SalesBuilder s = new SalesBuilder();
                //RetailPurchaseBuilder rpb = new RetailPurchaseBuilder();
                //RetailSalesBuilder rsb = new RetailSalesBuilder();

                DataFunction(d);
                //PurchaseFunction(p);
                //SalesFunction(s);
                //RetailPurchaseFunction(rpb);
                //RetailSalesFunction(rsb);
            }
        }

        public static void PurchaseFunction(PurchaseBuilder p)
        {
            p.PopulateData();

            //---

        }

        public static void SalesFunction(SalesBuilder s)
        {
            s.PopulateData();
        }

        public static void RetailSalesFunction(RetailSalesBuilder rsb)
        {
            rsb.PopulateData();

            // ---
            Receivable receivables1 = rsb._receivableService.GetObjectBySource(Core.Constants.Constant.ReceivableSource.RetailSalesInvoice, rsb.rsi1.Id);
            Receivable receivables2 = rsb._receivableService.GetObjectBySource(Core.Constants.Constant.ReceivableSource.RetailSalesInvoice, rsb.rsi2.Id);
            Receivable receivables3 = rsb._receivableService.GetObjectBySource(Core.Constants.Constant.ReceivableSource.RetailSalesInvoice, rsb.rsi3.Id);

            IList<ReceiptVoucherDetail> receiptVoucherDetails1 = rsb._receiptVoucherDetailService.GetObjectsByReceivableId(receivables1.Id);
            IList<ReceiptVoucherDetail> receiptVoucherDetails2 = rsb._receiptVoucherDetailService.GetObjectsByReceivableId(receivables2.Id);
            IList<ReceiptVoucherDetail> receiptVoucherDetails3 = rsb._receiptVoucherDetailService.GetObjectsByReceivableId(receivables3.Id);

            foreach (var receiptVoucherDetail in receiptVoucherDetails1)
            {
                if (!receiptVoucherDetail.IsConfirmed) Console.WriteLine("1:FALSE");
            }

            foreach (var receiptVoucherDetail in receiptVoucherDetails2)
            {
                if (!receiptVoucherDetail.IsConfirmed) Console.WriteLine("2:FALSE");
            }

            foreach (var receiptVoucherDetail in receiptVoucherDetails3)
            {
                if (!receiptVoucherDetail.IsConfirmed) Console.WriteLine("3:FALSE");
            }
            // ---
            rsb._retailSalesInvoiceService.UnpaidObject(rsb.rsi1, rsb._receiptVoucherService, rsb._receiptVoucherDetailService, 
                                                        rsb._cashBankService, rsb._receivableService, rsb._cashMutationService);
            rsb._retailSalesInvoiceService.UnpaidObject(rsb.rsi2, rsb._receiptVoucherService, rsb._receiptVoucherDetailService,
                                                        rsb._cashBankService, rsb._receivableService, rsb._cashMutationService);
            rsb._retailSalesInvoiceService.UnpaidObject(rsb.rsi3, rsb._receiptVoucherService, rsb._receiptVoucherDetailService,
                                                        rsb._cashBankService, rsb._receivableService, rsb._cashMutationService);

            rsb._retailSalesInvoiceService.UnconfirmObject(rsb.rsi1, rsb._retailSalesInvoiceDetailService, rsb._receivableService, 
                                                           rsb._receiptVoucherDetailService, rsb._warehouseItemService, rsb._warehouseService, 
                                                           rsb._itemService, rsb._barringService, rsb._stockMutationService);
            rsb._retailSalesInvoiceService.UnconfirmObject(rsb.rsi2, rsb._retailSalesInvoiceDetailService, rsb._receivableService,
                                                           rsb._receiptVoucherDetailService, rsb._warehouseItemService, rsb._warehouseService,
                                                           rsb._itemService, rsb._barringService, rsb._stockMutationService);
            rsb._retailSalesInvoiceService.UnconfirmObject(rsb.rsi3, rsb._retailSalesInvoiceDetailService, rsb._receivableService,
                                                           rsb._receiptVoucherDetailService, rsb._warehouseItemService, rsb._warehouseService,
                                                           rsb._itemService, rsb._barringService, rsb._stockMutationService);
        }

        public static void RetailPurchaseFunction(RetailPurchaseBuilder rpb)
        {
            rpb.PopulateData();

            // ---
            Payable payables1 = rpb._payableService.GetObjectBySource(Core.Constants.Constant.PayableSource.RetailPurchaseInvoice, rpb.rpi1.Id);
            Payable payables2 = rpb._payableService.GetObjectBySource(Core.Constants.Constant.PayableSource.RetailPurchaseInvoice, rpb.rpi2.Id);
            Payable payables3 = rpb._payableService.GetObjectBySource(Core.Constants.Constant.PayableSource.RetailPurchaseInvoice, rpb.rpi3.Id);

            IList<PaymentVoucherDetail> paymentVoucherDetails1 = rpb._paymentVoucherDetailService.GetObjectsByPayableId(payables1.Id);
            IList<PaymentVoucherDetail> paymentVoucherDetails2 = rpb._paymentVoucherDetailService.GetObjectsByPayableId(payables2.Id);
            IList<PaymentVoucherDetail> paymentVoucherDetails3 = rpb._paymentVoucherDetailService.GetObjectsByPayableId(payables3.Id);

            foreach (var paymentVoucherDetail in paymentVoucherDetails1)
            {
                if (!paymentVoucherDetail.IsConfirmed) Console.WriteLine("1:FALSE");
            }

            foreach (var paymentVoucherDetail in paymentVoucherDetails2)
            {
                if (!paymentVoucherDetail.IsConfirmed) Console.WriteLine("2:FALSE");
            }

            foreach (var paymentVoucherDetail in paymentVoucherDetails3)
            {
                if (!paymentVoucherDetail.IsConfirmed) Console.WriteLine("3:FALSE");
            }
            // ---
            rpb._retailPurchaseInvoiceService.UnpaidObject(rpb.rpi1, rpb._paymentVoucherService, rpb._paymentVoucherDetailService,
                                                        rpb._cashBankService, rpb._payableService, rpb._cashMutationService);
            rpb._retailPurchaseInvoiceService.UnpaidObject(rpb.rpi2, rpb._paymentVoucherService, rpb._paymentVoucherDetailService,
                                                        rpb._cashBankService, rpb._payableService, rpb._cashMutationService);
            rpb._retailPurchaseInvoiceService.UnpaidObject(rpb.rpi3, rpb._paymentVoucherService, rpb._paymentVoucherDetailService,
                                                        rpb._cashBankService, rpb._payableService, rpb._cashMutationService);

            rpb._retailPurchaseInvoiceService.UnconfirmObject(rpb.rpi1, rpb._retailPurchaseInvoiceDetailService, rpb._payableService,
                                                           rpb._paymentVoucherDetailService, rpb._warehouseItemService, rpb._warehouseService,
                                                           rpb._itemService, rpb._barringService, rpb._stockMutationService);
            rpb._retailPurchaseInvoiceService.UnconfirmObject(rpb.rpi2, rpb._retailPurchaseInvoiceDetailService, rpb._payableService,
                                                           rpb._paymentVoucherDetailService, rpb._warehouseItemService, rpb._warehouseService,
                                                           rpb._itemService, rpb._barringService, rpb._stockMutationService);
            rpb._retailPurchaseInvoiceService.UnconfirmObject(rpb.rpi3, rpb._retailPurchaseInvoiceDetailService, rpb._payableService,
                                                           rpb._paymentVoucherDetailService, rpb._warehouseItemService, rpb._warehouseService,
                                                           rpb._itemService, rpb._barringService, rpb._stockMutationService);
        }

        public static void DataFunction(DataBuilder d)
        {
            d.PopulateData();

            // End of Test
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();
        }
    }
}
