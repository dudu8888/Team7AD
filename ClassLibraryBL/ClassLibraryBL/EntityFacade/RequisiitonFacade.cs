﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibraryBL.Entities;
namespace ClassLibraryBL.EntityFacade
{
    class RequisiitonFacade
    {
        LogicUnivSystemEntities luse = new LogicUnivSystemEntities();

        public List<requisition> getPendingRequisition(User u)
        {
            var t = (from x in luse.requisitions
                     where x.status == "Pending" && x.departmentId == u.DepartmentId
                     select x).ToList();
            return t;
        }

        public List<RequisitionDetails> getRequisitionDetails(string rid)
        {
           int reqid = int.Parse(rid);
           List<RequisitionDetails> rd = new List<RequisitionDetails>();
           var ilist = (from x in luse.requisitions
                         from y in luse.items
                         from z in luse.requsiiton_item
                         from l in luse.departments
                         from m in luse.collectionPoints
                         from n in luse.users
                         where x.requisitionId == z.requisitionId && y.itemId == z.itemId && x.requisitionId == reqid && x.departmentId == l.departmentId && x.userId == n.userId && l.collectionPointId == m.collectionPointId 
                         select new RequisitionDetails {
                           Description = y.description, 
                           Number = z.requestQty,
                           Unit = y.unit,
                           Status = x.status,
                           CollectionPoint = m.address,
                           ReqDate = x.requestDate,
                           Name = n.name,
                           photourl = "../images/" + y.description.Trim() + ".jpg"
                         }).ToList();
            return ilist;

        }

        public void approveRequisition(int x)
        {
            var req = (from y in luse.requisitions
                       where y.requisitionId == x
                        select y).First();
            req.status = "Approved";
            luse.SaveChanges();



        }
        public void rejectRequisition(int x,string reason)
        {
            var req = (from y in luse.requisitions
                       where y.requisitionId == x
                       select y).First();
            req.status = "Rejected";
            req.rejectReason = reason;
            luse.SaveChanges();
        }
        public List<requisition> getPreRequisition(User u)
        {
            var t = (from x in luse.requisitions
                     where x.status == "Approved"
                     select x).ToList();
            return t;
        }


        public List<requisition> getAllRequisitionEmployee(User u)
        {
            var t = (from a in luse.requisitions
                     where a.userId == u.UserId
                     select a).ToList();
            return t;
        }

        public List<requisition> getPendingRequisitionEmployee(User u)
        {
            var t = (from a in luse.requisitions
                     where a.userId == u.UserId && a.status.Trim() == "Pending"
                     select a).ToList();
            return t;
        }

        public void addRequisition(User u, List<ShoppingItem> sclist)
        {
            requisition re = new requisition();
            re.departmentId = u.DepartmentId;
            re.userId = u.UserId;
            re.rejectReason = null;
            re.status = "Pending";
            re.requestDate = DateTime.Now;
            luse.requisitions.Add(re);
            luse.SaveChanges();
            for (int i = 0; i < sclist.Count; i++) {
                requsiiton_item reItem = new requsiiton_item();
                reItem.requisitionId = re.requisitionId;
                reItem.itemId = sclist[i].ItemId;
                reItem.requestQty = sclist[i].Amount;
                luse.requsiiton_item.Add(reItem);
                luse.SaveChanges();
            }


        }
        public void checkItemAvailable(int rid)
        {
            int flag = 0;
            ArrayList itemidlistNotAvailable = new ArrayList();
            List<itemValidate> itemidlistAvailable = new List<itemValidate>();
            var n = (from x in luse.requisitions
                     from y in luse.items
                     from z in luse.requsiiton_item
                     where x.requisitionId == z.requisitionId && y.itemId == z.itemId && x.requisitionId == rid
                     select new itemValidate{ 
                         Itemid = z.itemId,
                         RequestQty = z.requestQty,
                         StockBalance = y.balance
                     }).ToList();
            List<itemValidate> itemlist = n;
            foreach (itemValidate x in itemlist)
            {
                if (x.StockBalance < x.RequestQty)
                {
                    itemidlistNotAvailable.Add(x.Itemid);
                    flag++;
                }
                else
                {
                    itemidlistAvailable.Add(x);
                }
            }
            if (flag == 0)
            {
                var m = (from l in luse.requisitions
                         where l.requisitionId == rid
                         select l).First();
                m.status = "WaitingCollection";
                foreach (itemValidate x in itemlist)
                {
                    var u = (from x1 in luse.items
                             where x1.itemId == x.Itemid
                             select x1).First();
                    u.balance = u.balance - x.RequestQty;
                }
                luse.SaveChanges();
            }
            else
            {
                var m = (from l in luse.requisitions
                         where l.requisitionId == rid
                         select l).First();
                m.status = "PendingForOrder";
                foreach (int x in itemidlistNotAvailable)
                {
                    var u = (from x1 in luse.items
                             where x1.itemId == x
                             select x1).First();
                    u.status = "needReorderSoon";
                }
                if (itemidlistAvailable.Count != 0)
                {
                foreach (itemValidate x in itemidlistAvailable)
                {
                    var u = (from x1 in luse.items
                             where x1.itemId == x.Itemid
                             select x1).First();
                    u.balance = u.balance - x.RequestQty;
                }
                }
                luse.SaveChanges();
            }
        }
        


        //**********************************DU DU********************************
        static DateTime dt = DateTime.Now;
        static int weeknow = Convert.ToInt32(DateTime.Now.DayOfWeek);
        static int dayspan = (-1) * weeknow;
        DateTime dt2 = dt.AddMonths(1);
        DateTime monday = DateTime.Now.AddDays(dayspan);
        public Object getRequisition()
        {
            var data = from r in luse.requisitions
                       join d in luse.departments on r.departmentId equals d.departmentId
                       where r.status != "Completed"
                       where r.status != "Reject"
                       where r.requestDate >= monday.Date
                       select new { r.requisitionId, r.requestDate, d.deptName, r.status };
            Object o = data.ToList();
            return o;
        }
        //**********************************DU DU********************************

        
        //**** Peng xiao meng ********************
        public List<RequisitionMix> currentweekbyitem()
        {
            DateTime da = DateTime.Today.Date;
            String dw = da.DayOfWeek.ToString();

            switch (dw)
            {
                case "Tuesday":
                    da = da.AddDays(-1);
                    break;
                case "Wednesday":
                    da = da.AddDays(-2);
                    break;
                case "Thursday":
                    da = da.AddDays(-3);
                    break;
                case "Friday":
                    da = da.AddDays(-4);
                    break;
                case "Saturday":
                    da = da.AddDays(-5);
                    break;
                case "Sunday":
                    da = da.AddDays(-6);
                    break;
                default:
                    break;
            }

            var n = from a in luse.requisitions
                    join b in luse.requsiiton_item on a.requisitionId equals b.requisitionId
                    where a.requestDate > da && a.status =="WaitingCollection"
                    group b.requisition_itemId by b.itemId into g
                    join c in luse.items on g.Key equals c.itemId
                    join d in luse.categories on c.categoryId equals d.categoryId
                    select new RequisitionMix
                    {
                        itemID = g.Key,
                        Category = d.categoryName,
                        Itemname = c.description,
                        amount = g.Count(),
                        Unit = c.unit,
                        Bin = c.binNumber
                    };
            return n.ToList();
        }
        public List<RequisitionMix> currentweekbydepartment(string s)
        {
            DateTime da = DateTime.Today.Date;
            String dw = da.DayOfWeek.ToString();
            switch (dw)
            {
                case "Tuesday":
                    da = da.AddDays(-1);
                    break;
                case "Wednesday":
                    da = da.AddDays(-2);
                    break;
                case "Thursday":
                    da = da.AddDays(-3);
                    break;
                case "Friday":
                    da = da.AddDays(-4);
                    break;
                case "Saturday":
                    da = da.AddDays(-5);
                    break;
                case "Sunday":
                    da = da.AddDays(-6);
                    break;
                default:
                    break;
            }
            var n = from a in luse.requisitions
                    join b in luse.requsiiton_item on a.requisitionId equals b.requisitionId
                    where (a.requestDate > da) && (a.departmentId == s)
                    group b.requisition_itemId by b.itemId into g
                    join c in luse.items on g.Key equals c.itemId
                    join d in luse.categories on c.categoryId equals d.categoryId
                    select new RequisitionMix
                    {
                        itemID = g.Key,
                        Category = d.categoryName,
                        Itemname = c.description,
                        amount = g.Count(),
                        Unit = c.unit,
                        Bin = c.binNumber

                    };
            return n.ToList();
        }
        public List<RequisitionMix> itemwithoutdate()
        {
            var n = from a in luse.requisitions
                    join b in luse.requsiiton_item on a.requisitionId equals b.requisitionId
                    group b.requisition_itemId by b.itemId into g
                    join c in luse.items on g.Key equals c.itemId
                    join d in luse.categories on c.categoryId equals d.categoryId
                    select new RequisitionMix
                    {
                        itemID = g.Key,
                        Category = d.categoryName,
                        Itemname = c.description,
                        amount = g.Count(),
                        Unit = c.unit,
                        Bin = c.binNumber

                    };
            return n.ToList();
        }

        public List<RequisitionMix> itemwithdate(string start, string end)
        {

            DateTime dt1 = DateTime.ParseExact(start, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentCulture);
            DateTime dt2 = DateTime.ParseExact(end, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentCulture);
            var n = from a in luse.requisitions
                    join b in luse.requsiiton_item on a.requisitionId equals b.requisitionId
                    where (a.requestDate < dt2) && (a.requestDate > dt1)
                    group b.requisition_itemId by b.itemId into g
                    join c in luse.items on g.Key equals c.itemId
                    join d in luse.categories on c.categoryId equals d.categoryId
                    select new RequisitionMix
                    {
                        itemID = g.Key,
                        Category = d.categoryName,
                        Itemname = c.description,
                        amount = g.Count(),
                        Unit = c.unit,
                        Bin = c.binNumber

                    };
            return n.ToList();

        }
        public List<RequisitionMix> departmentwithoutdate(string ts)
        {
            var n = from a in luse.requisitions
                    join b in luse.requsiiton_item on a.requisitionId equals b.requisitionId
                    where (a.departmentId == ts)
                    group b.requisition_itemId by b.itemId into g
                    join c in luse.items on g.Key equals c.itemId
                    join d in luse.categories on c.categoryId equals d.categoryId
                    select new RequisitionMix
                    {
                        itemID = g.Key,
                        Category = d.categoryName,
                        Itemname = c.description,
                        amount = g.Count(),
                        Unit = c.unit,
                        Bin = c.binNumber

                    };

            {
                return n.ToList();
            }
        }
        public List<RequisitionMix> departmentwithdate(string ts, string start, string end)
        {

            DateTime dt1 = DateTime.ParseExact(start, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentCulture);
            DateTime dt2 = DateTime.ParseExact(end, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentCulture);
            var n = from a in luse.requisitions
                    join b in luse.requsiiton_item on a.requisitionId equals b.requisitionId
                    where (a.departmentId == ts) && (a.requestDate < dt2) && (a.requestDate > dt1)
                    group b.requisition_itemId by b.itemId into g
                    join c in luse.items on g.Key equals c.itemId
                    join d in luse.categories on c.categoryId equals d.categoryId
                    select new RequisitionMix
                    {
                        itemID = g.Key,
                        Category = d.categoryName,
                        Itemname = c.description,
                        amount = g.Count(),
                        Unit = c.unit,
                        Bin = c.binNumber
                    };
            return n.ToList();
        }

        //**********************peng xiao meng******************



    }
}
