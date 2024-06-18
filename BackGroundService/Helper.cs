using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackGroundService
{
    internal class Helper
    {
        public List<LCC_pnr> GetTickets()
        {
            try
            {
                List<LCC_pnr> lst = new List<LCC_pnr>();
                var result = ExecuteStoredProcedure("sp_GetIndigoLCCTickets");
                lst = result.ToList<LCC_pnr>();
                return lst;
            }
            catch (Exception ex)
            {
                SendExceptionEmail(ex);
                throw;
            }
        }

        public void LogError(Exception ex, string pnr, string lastName)
        {
            string cs = "Data Source=11.11.11.169,8081;Initial Catalog=Audit;Persist Security Info=True;User ID=IATA_Cred;Password=Riya@12345";
            SqlConnection con = new SqlConnection(cs);
            string AddDetails = "Insert into LogError_OpenticktCancel_Indigo(errorMsg,stackTrace,pnr_number,lastname) values(@errorMsg,@stackTrace,@pnr_number,@lastname)";
            SqlCommand cmd = new SqlCommand(AddDetails, con);
            cmd.Parameters.AddWithValue("@errorMsg", ex.Message);
            cmd.Parameters.AddWithValue("@stackTrace", ex.StackTrace);
            cmd.Parameters.AddWithValue("@pnr_number", pnr);
            cmd.Parameters.AddWithValue("@lastname", lastName);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
        }

        public List<Pnr_SegmentDetails> CheckPnr(string pnr)
        {
            try
            {
                List<Pnr_SegmentDetails> lst = new List<Pnr_SegmentDetails>();
                SqlParameter param1 = new SqlParameter("@pnr_number", SqlDbType.VarChar, 50);
                param1.Value = pnr;

                var result = ExecuteStoredProcedure("sp_checkduplicatePNR", param1);
                lst = result.ToList<Pnr_SegmentDetails>();
                return lst;
            }
            catch (Exception ex)
            {
                SendExceptionEmail(ex);
                throw;
            }

        }

        public void UpdateRefund(string refundAmount, int entry_number)
        {
            try
            {
                string cs = "Data Source=11.11.11.169,8081;Initial Catalog=Audit;Persist Security Info=True;User ID=IATA_Cred;Password=Riya@12345";
                SqlConnection con = new SqlConnection(cs);
                string AddDetails = "update Audit.dbo.LCC_pnr_segment_Details set refundAmount = @refundAmount , refundDate = getdate(),isProcessedBy = 'openTicketCancellation_Indigo',ProcessDate = getdate() where entry_number = @entry_number";
                SqlCommand cmd = new SqlCommand(AddDetails, con);
                cmd.Parameters.AddWithValue("@refundAmount", refundAmount);
                cmd.Parameters.AddWithValue("@entry_number", entry_number);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void UpdateStatus(string pnr_number, string lastname)
        {
            try
            {
                string cs = "Data Source=11.11.11.169,8081;Initial Catalog=Audit;Persist Security Info=True;User ID=IATA_Cred;Password=Riya@12345";
                SqlConnection con = new SqlConnection(cs);
                string AddDetails = "update Audit.dbo.LCC_pnr_segment_Details set isProcessedBy = 'openTicketCancellation_Indigo',ProcessDate = getdate() where pnr_number = @pnr_number  and lastname = @lastname";
                SqlCommand cmd = new SqlCommand(AddDetails, con);
                cmd.Parameters.AddWithValue("@pnr_number", pnr_number);
                cmd.Parameters.AddWithValue("@lastname", lastname);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception)
            {
                throw;
            }

        }

        public void SendExceptionEmail(Exception ex)
        {
            try
            {
                string FromMail = "no-reply@riya.travel";
                string displayName = "Indigo Refund Scrapping";
                string ToEmail = "developers@riya.travel";
                string body = "Hello,\r\n\r\n" +
                        "An exception has occurred in Indigo Refund Scrapping at " + DateTime.Now + ".\r\n\r\n" +
                        "Exception Details:\r\n" +
                        "-----------------------------------------\r\n" +
                        "Exception Type: " + ex.GetType().Name + "\r\n" +
                        "Message: " + ex.Message + "\r\n" +
                        "Source: " + ex.Source + "\r\n" +
                        "Stack Trace: " + ex.StackTrace + "\r\n\r\n";

                string subject = "Exception in Indigo Refund Scrapping";
                MailAddress sendFrom = new MailAddress(FromMail, displayName);
                MailAddress sendTo = new MailAddress(ToEmail);
                MailMessage nmsg = new MailMessage(sendFrom, sendTo);

                nmsg.Subject = subject;
                nmsg.IsBodyHtml = false;
                nmsg.Body = body;

                SmtpClient smtpClient = new SmtpClient();
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Host = "us1-mta1.sendclean.net";
                smtpClient.Port = 587;
                string smtpUserName = "smtp33800993";
                string smtpPassword = "9Hh4PLQIgn";
                smtpClient.Credentials = new NetworkCredential(smtpUserName, smtpPassword);
                smtpClient.EnableSsl = false;
                smtpClient.Send(nmsg);
            }
            catch (Exception)
            {

                throw;
            }
        }


        public void SendSuccessEmail()
        {
            try
            {
                string FromMail = "no-reply@riya.travel";
                string displayName = "Indigo Refund Success";
                string ToEmail = "mayur.riya04@gmail.com";
                string body = "Hello,\r\n\r\n" +
                        "Background Service Completed Successfully at" + DateTime.Now + ".\r\n\r\n";


                string subject = "Background Service Completed Successfully";
                MailAddress sendFrom = new MailAddress(FromMail, displayName);
                MailAddress sendTo = new MailAddress(ToEmail);
                MailMessage nmsg = new MailMessage(sendFrom, sendTo);

                nmsg.Subject = subject;
                nmsg.IsBodyHtml = false;
                nmsg.Body = body;

                SmtpClient smtpClient = new SmtpClient();
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Host = "us1-mta1.sendclean.net";
                smtpClient.Port = 587;
                string smtpUserName = "smtp33800993";
                string smtpPassword = "9Hh4PLQIgn";
                smtpClient.Credentials = new NetworkCredential(smtpUserName, smtpPassword);
                smtpClient.EnableSsl = false;
                smtpClient.Send(nmsg);
            }
            catch (Exception)
            {

                throw;
            }
        }


        public static DataTable ExecuteStoredProcedure(string storedProcedureName, params SqlParameter[] parameters)
        {
            // Get the connection string from the configuration file
            string cs = "Data Source=11.11.11.169,8081;Initial Catalog=Audit;Persist Security Info=True;User ID=IATA_Cred;Password=Riya@12345";

            // Create a new SqlConnection
            using (SqlConnection con = new SqlConnection(cs))
            {
                // Create a new SqlCommand
                using (SqlCommand cmd = new SqlCommand(storedProcedureName, con))
                {
                    // Set the command type to stored procedure
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Add any parameters to the command
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.Add(param);
                        }
                    }

                    // Open the connection
                    con.Open();

                    // Execute the command and fill the results into a DataTable
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }

    }
}
