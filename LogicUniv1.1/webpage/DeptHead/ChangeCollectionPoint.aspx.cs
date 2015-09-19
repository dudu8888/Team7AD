﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClassLibraryBL.EntityFacade;
using ClassLibraryBL.Controller.DeptHead;
using ClassLibraryBL.Entities;
namespace LogicUniv1._1.webpage.DeptHead
{

    public partial class ChangeCollectionPoint : System.Web.UI.Page
    {
       
        MaintainCollectionPointController mcpc = new MaintainCollectionPointController();
        NewMessageController nmc = new NewMessageController();
        protected void Page_Load(object sender, EventArgs e)
        {
            User u = (User)Session["UserEntity"];
            if (u == null || u.RoleId != 1)
            {
                Response.Redirect("../Security.aspx");
            }

            getCollectionPointTime(u);

            

            Session["newmsg"] = nmc.getAllmessage(u);

            int msgCount = nmc.getAllmessage(u).Count();
            msgcount2.Text = msgCount.ToString();
            NewMessage.Text = "You have " + msgCount + " new message";



        }
        public void getCollectionPointTime(User u)
        {
           GridView1.DataSource= mcpc.getCollectionPointTime(u);
           GridView1.DataBind();

        }


        protected void ConfirmBtn_Click(object sender, EventArgs e)
        {
            User u = (User)Session["UserEntity"];
            
            string collectionP = GridView1.SelectedRow.Cells[0].Text.Trim();
            string deptid = u.DepartmentId;
            mcpc.changeCollectionPoint(u, collectionP);
            mcpc.mailNotificationCollectionPoint(u, collectionP, deptid);
            Label2.Text = "Successfuly changed, Email has been sent to all relevant individual.";
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            GridView1.DataBind();
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Label1.Text = GridView1.SelectedRow.Cells[0].Text.Trim();

            //ClientScript.RegisterStartupScript(this.GetType(), "why", "('#myModal').myModalLabel({})", true);
        }
    }
}