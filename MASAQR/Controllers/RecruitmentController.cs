using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MASAQR.Controllers
{
    /// <summary>
    /// Recruitment Controller Header
    /// </summary>
    public class RecruitmentController : ApiController
    {
        private bool IfFailed(int reg)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.FirstOrDefault(e => e.Reg_No == reg);

                if (res.Interview != null && res.Reference_1 != null && res.Reference_2 != null && res.ABET_Lit != null && res.ABET_Num != null && res.Crime_Check != null && res.Master_Contract != null)
                {
                    res.Vetting_Finished = true;
                }

                if (res.Crime_Check == "Positive")
                {
                    res.Decline_Reason = "Positive Crime Check";
                    res.Vetting_Finished = true;
                    entities.SaveChanges();
                    return false;
                }

                if(res.ABET_Lit < 40 || res.ABET_Num < 40)
                {
                    res.Decline_Reason = "ABET Scores too Low";
                    res.Vetting_Finished = true;
                    entities.SaveChanges();
                    return false;
                }

                if(res.Reference_1 <= 5 || res.Reference_2 <= 5)
                {
                    res.Decline_Reason = "Reference Scores too Low";
                    res.Vetting_Finished = true;
                    entities.SaveChanges();
                    return false;
                }

                if(res.Interview <= 5)
                {
                    res.Decline_Reason = "Interview Score too Low";
                    res.Vetting_Finished = true;
                    entities.SaveChanges();
                    return false;
                }

                if(res.Manager_Approval == "Decline")
                {
                    res.Decline_Reason = "Manager Declined";
                    res.Employment_Status = "Poor MASA Feedback";
                    res.Vetting_Finished = true;
                    entities.SaveChanges();
                    return false;
                }
                
                res.Decline_Reason = null;
                entities.SaveChanges();
                return true;
            }

        }

        /// <summary>
        /// Get all recruitment data
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Get()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var query = entities.RecruitmentTable.ToList();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, query);
            }
        }

        /// <summary>
        /// Get Info with Registration Number
        /// </summary>
        /// <param name="id">Registration Number</param>
        /// <returns></returns>
        [Route("api/Recruitment/GetID/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [HttpGet]
        public HttpResponseMessage GetID(int id)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.FirstOrDefault(e => e.Reg_No == id);
                if (res != null)
                {
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error, User not Found");
                }
            }
        }


        /// <summary>
        /// Get Batch of Crime Checks Needed
        /// </summary>
        /// <returns></returns>
        [Route("api/Recruitment/BatchCrime")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [HttpGet]
        public HttpResponseMessage BatchCrime()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.Where(e => (String.IsNullOrEmpty(e.Crime_Check) || e.Crime_Check.Equals("Pending") || e.Crime_Check.Equals("NR")) && e.Initial_Impression.Equals("Approved")).ToList();
                if (res != null)
                {
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error, User not Found");
                }
            }
        }


        /// <summary>
        /// Get Batch Tax Numbers Needed
        /// </summary>
        /// <returns></returns>
        [Route("api/Recruitment/BatchTax")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [HttpGet]
        public HttpResponseMessage BatchTax()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.Where(e => String.IsNullOrEmpty(e.Tax_Number) && e.Initial_Impression.Equals("Approved")).ToList();
                if (res != null)
                {
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error, User not Found");
                }
            }
        }

        /// <summary>
        /// Get Batch of Master Contracts Needed
        /// </summary>
        /// <returns></returns>
        [Route("api/Recruitment/BatchContract")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [HttpGet]
        public HttpResponseMessage BatchContract()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.Where(e => e.Master_Contract != true && e.Initial_Impression.Equals("Approved")).ToList();
                if (res != null)
                {
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error, User not Found");
                }
            }
        }

        /// <summary>
        /// Get Batch of Users that need Approval
        /// </summary>
        /// <returns></returns>
        [Route("api/Recruitment/BatchApproval")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [HttpGet]
        public HttpResponseMessage BatchApproval(int? day = null)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.ToList();
                if(day != null && day != 0 && day != 7)
                {
                    var date = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)day);
                    res = entities.RecruitmentTable.Where(e => e.Date_Added == date && String.IsNullOrEmpty(e.Manager_Approval) && e.Initial_Impression.Equals("Approved") && e.ABET_Lit > 39 && e.ABET_Num > 39 && e.Interview > 5 &&
                e.Reference_1 > 5 && e.Reference_2 > 5 && e.Crime_Check.Equals("Clear") && e.Master_Contract == true).ToList();

                }
                else if(day != null && day == 7)
                {
                    var date = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
                    res = entities.RecruitmentTable.Where(e => e.Date_Added >= date && String.IsNullOrEmpty(e.Manager_Approval) && e.Initial_Impression.Equals("Approved") && e.ABET_Lit > 39 && e.ABET_Num > 39 && e.Interview > 5 &&
                e.Reference_1 > 5 && e.Reference_2 > 5 && e.Crime_Check.Equals("Clear") && e.Master_Contract == true).ToList();

                }else
                {
                    var date = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
                    res = entities.RecruitmentTable.Where(e => e.Date_Added < date && String.IsNullOrEmpty(e.Manager_Approval) && e.Initial_Impression.Equals("Approved") && e.ABET_Lit > 39 && e.ABET_Num > 39 && e.Interview > 5 &&
                e.Reference_1 > 5 && e.Reference_2 > 5 && e.Crime_Check.Equals("Clear") && e.Master_Contract == true).ToList();
                }


                if (res != null)
                {
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error, User not Found");
                }
            }
        }

        /// <summary>
        /// Get Batch of Declined Users
        /// </summary>
        /// <returns></returns>
        [Route("api/Recruitment/BatchDeclined")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [HttpGet]
        public HttpResponseMessage BatchDeclined(int? day = null)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {

                var res = entities.RecruitmentTable.ToList();
                if (day != null && day != 0 && day != 7)
                {
                    var date = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)day);
                    res = entities.RecruitmentTable.Where(e => e.Date_Added == date && e.Decline_Reason != null).ToList();

                }else if(day != null && day == 7)
                {
                    var date = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
                    res = entities.RecruitmentTable.Where(e => e.Date_Added >= date && e.Decline_Reason != null).ToList();
                }
                else
                {
                    var date = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
                    res = entities.RecruitmentTable.Where(e => e.Date_Added >= date && e.Decline_Reason != null).ToList();
                }


                if (res != null)
                {
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error, User not Found");
                }
            }
        }

        /// <summary>
        /// Add Tax Number
        /// </summary>
        /// <param name="user">User ID</param>
        /// <param name="reg_no">Registration Number</param>
        /// <param name="tax">Tax Number</param>
        /// <returns></returns>
        [Route("api/Recruitment/TaxChange/{user}/{reg_no}/{tax}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [HttpGet]
        public HttpResponseMessage TaxChange(int user, int reg_no, String tax)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.FirstOrDefault(e => e.Reg_No == reg_no);

                Track_Modifications tm = new Track_Modifications()
                {
                    ID = user,
                    Modified_Action = "Added Tax Number",
                    Modified_Date = DateTime.Now,
                    Modified_Registration_No = reg_no,
                    Modified_Table = "Recruitment"
                };

                if (res != null)
                {
                    res.Tax_Number = tax;
                    res.Last_Modified = DateTime.Today;
                    entities.Track_Modifications.Add(tm);
                    entities.SaveChanges();
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error, User not Found");
                }
            }
        }

        /// <summary>
        /// Add Crime Check
        /// </summary>
        /// <param name="reg_no"></param>
        /// <param name="crime"></param>
        /// <returns></returns>
        [Route("api/Recruitment/CrimeChange/{reg_no}/{crime}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [HttpGet]
        public HttpResponseMessage CrimeChange(int reg_no, String crime)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.FirstOrDefault(e => e.Reg_No == reg_no);
                if (res != null)
                {
                    res.Crime_Check = crime;
                    res.Last_Modified = DateTime.Today;
                    Crim_Check cc = new Crim_Check()
                    {
                        Registration_No = reg_no,
                        Criminal_Result = crime,
                        Criminal_Date = DateTime.Today
                    };

                    entities.Crim_Check.Add(cc);
                    entities.SaveChanges();
                    bool h = IfFailed(reg_no);

                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error, User not Found");
                }
            }
        }

        /// <summary>
        /// Add Master Contract
        /// </summary>
        /// <param name="user">User ID</param>
        /// <param name="reg_no">Registration Number</param>
        /// <param name="contract">Contract</param>
        /// <returns></returns>
        [Route("api/Recruitment/ContractChange/{user}/{reg_no}/{contract}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [HttpGet]
        public HttpResponseMessage ContractChange(int user, int reg_no, bool contract)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.FirstOrDefault(e => e.Reg_No == reg_no);

                Track_Modifications tm = new Track_Modifications()
                {
                    ID = user,
                    Modified_Action = "Master Contract Signed",
                    Modified_Date = DateTime.Now,
                    Modified_Registration_No = reg_no,
                    Modified_Table = "Recruitment"
                };

                if (res != null)
                {
                    res.Master_Contract = contract;
                    res.Last_Modified = DateTime.Today;
                    entities.Track_Modifications.Add(tm);
                    entities.SaveChanges();
                    bool h = IfFailed(reg_no);
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error, User not Found");
                }
            }
        }

        /// <summary>
        /// Approve or Decline Candidate
        /// </summary>
        /// <param name="user">User ID</param>
        /// <param name="reg_no">Registration Number</param>
        /// <param name="approval">Approval or Decline</param>
        /// <returns></returns>
        [Route("api/Recruitment/ManagerDecision/{user}/{reg_no}/{approval}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [HttpGet]
        public HttpResponseMessage ManagerDecision(int user, int reg_no, String approval)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.FirstOrDefault(e => e.Reg_No == reg_no);
                // Get Login Details of Authority
                var getName = from f in entities.Login
                              where f.ID == user
                              select f.Name + " " + f.Surname;

                var name = getName.FirstOrDefault().ToString();

                Track_Modifications tm = new Track_Modifications()
                {
                    ID = user,
                    Modified_Action = "Manager Approval",
                    Modified_Date = DateTime.Now,
                    Modified_Registration_No = reg_no,
                    Modified_Table = "Recruitment"
                };

                if (res != null)
                {
                    res.Manager_Approval = approval;
                    res.Employment_Status = "Available";
                    res.Audit_By = name;
                    res.Last_Modified = DateTime.Today;
                    if (approval.Equals("Decline"))
                    {
                        tm.Modified_Action = "Manager Approval Declined";
                        res.Decline_Reason = "Manager Approval Declined";
                        res.Employment_Status = "Poor MASA Feedback";
                    }
                    entities.Track_Modifications.Add(tm);
                    entities.SaveChanges();
                    bool h = IfFailed(reg_no);
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error, User not Found");
                }
            }
        }

        /// <summary>
        /// Get User using ID or Passport
        /// </summary>
        /// <param name="id">ID Number</param>
        /// <param name="passport">Passport Number</param>
        /// <returns></returns>
        [Route("api/Recruitment/CheckUser")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [HttpGet]
        public HttpResponseMessage CheckUser(String id, String passport)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.FirstOrDefault(e => e.ID.Equals(id) || e.Passport.Equals(passport));
                if (res != null)
                {
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error, User not Found");
                }
            }
        }

        /// <summary>
        /// Get Last Registration Number
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [Route("api/Recruitment/RegNo")]
        [HttpGet]
        public HttpResponseMessage RegNo()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var ele = entities.RecruitmentTable.OrderByDescending(s => s.Reg_No).FirstOrDefault();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, ele);
            }
        }

        /// <summary>
        /// Post the new Registered Employee
        /// </summary>
        /// <param name="name">Name of Employee</param>
        /// <param name="surname">Surname of Employee</param>
        /// <param name="dob">Date of Birth</param>
        /// <param name="idNo">ID Number</param>
        /// <param name="passNo">Passport Number</param>
        /// <param name="workP">Work Permit Number</param>
        /// <param name="phone1">Contact Number</param>
        /// <param name="phone2">Contact Number 2</param>
        /// <param name="address">Home Address</param>
        /// <param name="area">Home Area</param>
        /// <param name="race">Race</param>
        /// <param name="gender">Gender</param>
        /// <param name="pos">Position(s)</param>
        /// <param name="impression">Initial Impression</param>
        /// <param name="decline">Decline Reason</param>
        /// <param name="contract">Master Contract Signed</param>
        /// <param name="empStat">Employment Status</param>
        /// <param name="audit">Audited By</param>
        /// <param name="vetted">Vetted By</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [Route("api/Recruitment/POST")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage POST(String name, String surname, DateTime dob, String idNo=null, String passNo=null, String workP = null, String phone1 = null, String phone2 = null, String address = null, String area = null,
            String race = null, String gender = null, String pos = null, String impression = null, bool? contract = null, String empStat = null, String audit = null, String vetted = null, String decline = null)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var trigger = false;

                if(impression.Equals("Declined"))
                {
                    empStat = "Unavailable";
                    decline.Replace('~', ',');
                }
                else
                {
                    trigger = true;
                }

                RecruitmentTable newOrder = new RecruitmentTable
                {
                    Name = name,
                    Surname = surname,
                    ID = idNo,
                    Date_of_Birth = dob,
                    Passport = passNo,
                    Work_Permit = workP,
                    Tel_1 = phone1,
                    Tel_2 = phone2,
                    Home_Address = address,
                    Area = area,
                    Race = race,
                    Gender = gender,
                    Positions = pos,
                    Initial_Impression = impression,
                    Decline_Reason = decline,
                    Master_Contract = contract,
                    Employment_Status = empStat,
                    Audit_By = audit,
                    Vetted_By = vetted,
                    Last_Modified = DateTime.Today,
                    Date_Added = DateTime.Today,
                    Vetting_Finished = false
                };

                entities.RecruitmentTable.Add(newOrder);
                entities.SaveChanges();

                if (trigger)
                {
                    var ele = entities.RecruitmentTable.OrderByDescending(s => s.Reg_No).FirstOrDefault();

                    Employee_Detail newDetails = new Employee_Detail
                    {
                        Registration_No = ele.Reg_No,
                        Name = ele.Name,
                        Nationality = null,
                        Home_Language = null,
                        Email = null,
                        Disabled = null,
                        Nature_of_Disability = null,
                        Martial_Status = null,
                        Health = null,
                        Vaccinated = null,
                        NoK_Name = null,
                        NoK_Contact = null,
                        NoK_Relationship = null,
                        Own_Transport = null,
                        Accident_History = null,
                        Accident_Detail = null
                    };

                    entities.Employee_Detail.Add(newDetails);
                    entities.SaveChanges();
                }

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, newOrder);

            }
        }

        /// <summary>
        /// Update Recruitment Data by Registration Number
        /// </summary>
        /// <param name="user">User ID</param>
        /// <param name="reg">Registration Number</param>
        /// <param name="abetL">ABET Litaracy</param>
        /// <param name="abetN">ABET Numeracy</param>
        /// <param name="interview">Interview</param>
        /// <param name="interviewType">Interview Type</param>
        /// <param name="ref1">Reference 1</param>
        /// <param name="refType1">Reference Type 1</param>
        /// <param name="ref2">Reference 2</param>
        /// <param name="refType2">Reference Type 2</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [Route("api/Recruitment/UpdateRecruitment")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage UpdateRecruitment(int user, int reg, int? abetL = null, int? abetN = null, int? interview = null, String interviewType = null, int? ref1 = null, String refType1 = null,
            int? ref2 = null, String refType2 = null)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.Where(e => e.Reg_No == reg).FirstOrDefault();
                var getName = from f in entities.Login
                           where f.ID == user
                           select f.Name + " " + f.Surname;

                var name = getName.FirstOrDefault().ToString();

                if (res != null)
                {
                    res.Interview = interview;
                    res.Interview_Type = interviewType;
                    res.Reference_1 = ref1;
                    res.Reference_Type_1 = refType1;
                    res.Reference_2 = ref2;
                    res.Reference_Type_2 = refType2;
                    res.ABET_Lit = abetL;
                    res.ABET_Num = abetN;
                    res.Vetted_By = name;
                    res.Last_Modified = DateTime.Today;

                    Track_Modifications tm = new Track_Modifications()
                    {
                        ID = user,
                        Modified_Action = "Updated Vetting",
                        Modified_Date = DateTime.Now,
                        Modified_Registration_No = reg,
                        Modified_Table = "Recruitment/References"
                    };

                    entities.Track_Modifications.Add(tm);

                    if (interview <= 5)
                    {
                        res.Decline_Reason = "Interview rating too low";
                        res.Vetting_Finished = true;
                    }

                    if(ref1 <= 5 || ref2 <= 5)
                    {
                        res.Decline_Reason = "Reference rating too low";
                        res.Vetting_Finished = true;
                    }

                    if(abetL <= 39 || abetN <= 39)
                    {
                        res.Decline_Reason = "ABET test score too low";
                        res.Vetting_Finished = true;
                    }

                    if(interview != null && ref1 != null && ref2 != null && abetL != null && abetN != null && res.Crime_Check != null && res.Master_Contract != null)
                    {
                        res.Vetting_Finished = true;
                    }

                    entities.SaveChanges();

                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new String[] { "Updated Successfully" });
                    //return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Recruitment Info was not Found");
                }
            }
        }

        /// <summary>
        /// Update the Quick Change Information
        /// </summary>
        /// <param name="reg">Registration Number</param>
        /// <param name="tele1">Tele 1</param>
        /// <param name="tele2">Tele 2</param>
        /// <param name="area">Area</param>
        /// <param name="empStatus">Employee Status</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [Route("api/Recruitment/QuickUpdate")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage QuickUpdate(int user, int reg, string tele1 = null, string tele2 = null, string area = null, string empStatus = null)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.Where(e => e.Reg_No == reg).FirstOrDefault();

                if (res != null)
                {
                    res.Tel_1 = tele1;
                    res.Tel_2 = tele2;
                    res.Area = area;
                    res.Employment_Status = empStatus;
                    res.Last_Modified = DateTime.Today;

                    Track_Modifications tm = new Track_Modifications()
                    {
                        ID = user,
                        Modified_Action = "Quick Update",
                        Modified_Date = DateTime.Now,
                        Modified_Registration_No = reg,
                        Modified_Table = "Recruitment"
                    };
                    entities.Track_Modifications.Add(tm);

                    entities.SaveChanges();

                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new String[] { "Updated Successfully" });
                    //return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Recruitment Info was not Found");
                }
            }
        }

        /// <summary>
        /// Update the Employees Details 
        /// </summary>
        /// <param name="reg"></param>
        /// <param name="name"></param>
        /// <param name="surname"></param>
        /// <param name="ID"></param>
        /// <param name="passport"></param>
        /// <param name="workP"></param>
        /// <param name="phone1"></param>
        /// <param name="phone2"></param>
        /// <param name="address"></param>
        /// <param name="area"></param>
        /// <param name="race"></param>
        /// <param name="gender"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [Route("api/Recruitment/UpdateEmployee")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage UpdateEmployee(int reg, String name, String surname, String ID, String passport, String workP, String phone1, String phone2, String address, String area, String race, String gender)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.Where(e => e.Reg_No == reg).FirstOrDefault();

                if (res != null)
                {
                    res.Name = name;
                    res.Surname = surname;
                    res.ID = ID;
                    res.Passport = passport;
                    res.Work_Permit = workP;
                    res.Tel_1 = phone1;
                    res.Tel_2 = phone2;
                    res.Home_Address = address;
                    res.Area = area;
                    res.Race = race;
                    res.Gender = gender;
                    res.Last_Modified = DateTime.Today;

                    entities.SaveChanges();

                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new String[] { "Updated Successfully" });
                    //return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "User not Found");
                }
            }
        }

        /// <summary>
        /// Update from the Recruitment Page
        /// </summary>
        /// <param name="reg_no">Registration Number</param>
        /// <param name="name">Employee Name</param>
        /// <param name="surname">Employee Surname</param>
        /// <param name="tax">Tax Number</param>
        /// <param name="empStat">Employment Status</param>
        /// <param name="race">Race</param>
        /// <param name="gender">Gender</param>
        /// <param name="phone1">Phone Number</param>
        /// <param name="phone2">Alternative Number</param>
        /// <param name="address">Home Address</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [Route("api/Recruitment/UpdateFullRecruitment")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage UpdateFullRecruitment(int user, int reg_no, String name = null, String surname = null, String tax = null, String empStat = null, String race = null, String gender = null, String phone1 = null, String phone2 = null, String address = null)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.Where(e => e.Reg_No == reg_no).FirstOrDefault();
                

                if (res != null)
                {
                    Track_Modifications tm = new Track_Modifications()
                    {
                        ID = user,
                        Modified_Action = "Updated Recruitment",
                        Modified_Date = DateTime.Now,
                        Modified_Registration_No = reg_no,
                        Modified_Table = "Recruitment"
                    };
                    
                    res.Name = name;
                    res.Surname = surname;
                    res.Tax_Number = tax;
                    res.Employment_Status = empStat;
                    res.Race = race;
                    res.Gender = gender;
                    res.Tel_1 = phone1;
                    res.Tel_2 = phone2;
                    res.Home_Address = address;
                    res.Last_Modified = DateTime.Today;

                    entities.Track_Modifications.Add(tm);
                    entities.SaveChanges();

                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new String[] { "Updated Successfully" });
                    //return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Order was not Found");
                }
            }
        }

        private DateTime CalculateAge(string idNumber)
        {
            var year = Convert.ToInt32(idNumber.Substring(0, 2));
            var month = Convert.ToInt32(idNumber.Substring(2, 2));
            var day = Convert.ToInt32(idNumber.Substring(4, 2));

            if (year < 50)
            {
                year = 2000 + year;
            }
            else
            {
                year = 1900 + year;
            }

            DateTime newDT = new DateTime(year, month, day);
            /*int age = 0;
            age = DateTime.Now.Subtract(newDT).Days;
            age = age / 365;*/

            return newDT;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RecruitmentTable))]
        [Route("api/Recruitment/SetDates")]
        [HttpGet]
        public HttpResponseMessage SetDates()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                foreach (var some in entities.RecruitmentTable.Where(e=> e.ID.Length > 2).ToList())
                {
                    some.Date_of_Birth = CalculateAge(some.ID);
                }

                entities.SaveChanges();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, entities.RecruitmentTable.ToList());
            }
        }

        /// <summary>
        /// Search Filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [Route("api/Recruitment/Search2/")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage Search2([FromUri] string[] filter)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.Where(e => e.Reg_No>0);

                var query = (from rt in entities.RecruitmentTable
                             where rt.Manager_Approval == "Approve"
                             let years = DateTime.Now.Year - rt.Date_of_Birth.Year
                             let birthYear = System.Data.Entity.DbFunctions.AddYears(rt.Date_of_Birth, years)

                             join pos in entities.Employee_Positions on rt.Reg_No equals pos.Register_No into A
                             from Employee_Positions in A.DefaultIfEmpty()

                             join lic in entities.Licence on rt.Reg_No equals lic.Registration_No into B
                             from Licence in B.DefaultIfEmpty()

                             join edu in entities.Education on rt.Reg_No equals edu.Registration_No into C
                             from Education in C.DefaultIfEmpty()

                             join disc in entities.Disciplinary on rt.Reg_No equals disc.Registration_No into D
                             from Disciplinary in D.DefaultIfEmpty()

                             join empD in entities.Employee_Detail on rt.Reg_No equals empD.Registration_No into E
                             from Employee_Detail in E.DefaultIfEmpty()
                             select new
                             {
                                 rt.Reg_No,
                                 rt.Employee_Number,
                                 rt.Name,
                                 rt.Surname,
                                 rt.ID,
                                 rt.Passport,
                                 Age = birthYear > DateTime.Now ? years-1 : years,
                                 rt.Home_Address,
                                 rt.Area,
                                 rt.Tel_1,
                                 rt.Tel_2,
                                 rt.Race,
                                 rt.Gender,
                                 rt.Tax_Number,
                                 rt.Employment_Status,
                                 Position = Employee_Positions == null ? String.Empty : Employee_Positions.Position,
                                 Experience = Employee_Positions == null ? String.Empty : Employee_Positions.Experience,
                                 Licence_Type = Licence == null ? String.Empty : Licence.Licence_Type,
                                 Licence_Code = Licence == null ? String.Empty : Licence.Licence_Code,
                                 Expiry_Date = Licence == null ? new DateTime() : Licence.Licence_Expiry_Date,
                                 Education_Category = Education == null ? String.Empty : Education.Category,
                                 Qualification = Education == null ? String.Empty : Education.Qualification,
                                 Disc_Count = D.Select(t => t.Registration_No).Distinct().Count(),
                                 Plea = Disciplinary == null ? String.Empty : Disciplinary.Plea,
                                 Sanction = Disciplinary == null ? String.Empty : Disciplinary.Sanction,
                                 Vaccinated = Employee_Detail == null ? String.Empty : Employee_Detail.Vaccinated
                             }).Distinct();

                string[] order = new string[10];
                int len = filter.Length;
                for(int f = 0; f < len; f++)
                {
                    int col = filter[f].IndexOf(":");
                    string sector = filter[f].Substring(0, col);
                    order[f] = sector;

                    switch (sector)
                    {
                        case "Position":
                            // Finds if the Position(s) are found
                            string positions = filter[f].Substring(col + 1);
                            string[] posArr = positions.Split('|');
                            query = query.Where(e => posArr.Contains(e.Position));
                            break;
                        case "Experience":
                            // Finds if the Position(s) are found
                            string exp = filter[f].Substring(col + 1);
                            if (!String.Equals(exp, "none"))
                            {
                                query = query.Where(e => e.Experience == exp);
                            }
                            break;
                        case "Licence":
                            // Finds if Licence Type matches the input
                            string lic = filter[f].Substring(col + 1);
                            string[] licArr = lic.Split('|');

                            query = query.Where(e => licArr.Contains(e.Licence_Type));
                            break;
                        case "Code":
                            // Finds if Licence Code matches the input
                            string cod = filter[f].Substring(col + 1);
                            if (!String.Equals(cod, "Any"))
                            {
                                string[] codeArr = cod.Split('|');

                                string[] arr = CodeCollect(codeArr);

                                query = query.Where(e => arr.Contains(e.Licence_Code));
                            }
                            break;
                        case "Qualification":
                            // Finds the Specified Catergory of Qualification
                            string qual = filter[f].Substring(col + 1);
                            string[] qualArr = qual.Split('|');
                            if (qual.Equals("Any"))
                            {
                                query = query.Where(e => e.Education_Category.Equals("Matric") || e.Education_Category.Equals("Tertiary") || e.Education_Category.Equals("Technical"));
                            }
                            else {
                                query = query.Where(e => qualArr.Contains(e.Education_Category));
                            }
                            break;
                        case "ETI":
                            // Finds the ages that match the ETI Condition
                            var min = 100;
                            var max = 0;

                            string eti = filter[f].Substring(col + 1);
                            string[] etiArr = eti.Split('|');
                            foreach(string ages in etiArr)
                            {
                                var tempMin = Convert.ToInt32(ages);
                                if(tempMin == 22)
                                {
                                    if(tempMin < min) { min = tempMin; }
                                    if(23 > max) { max = 23; }
                                }else if(tempMin == 24)
                                {
                                    if (tempMin < min) { min = tempMin; }
                                    if (27 > max) { max = 27; }
                                }
                                else if(tempMin == 28)
                                {
                                    if (tempMin < min) { min = tempMin; }
                                    if (29 > max) { max = 29; }
                                }
                            }
                            Debug.WriteLine("S:" + min + " L:" + max);

                            query = query.Where(a => a.Age >= min && a.Age <= max);
                            break;
                        case "Min":
                            // Finds the ages that are greater than Minimum Age
                            string age1 = filter[f].Substring(col + 1);
                            int minAge = Convert.ToInt32(age1);
                            
                            query = query.Where(a => a.Age >= minAge);
                            break;
                        case "Max":
                            // Finds the ages that are less than Maximum Age
                            string age2 = filter[f].Substring(col + 1);
                            int maxAge = Convert.ToInt32(age2);

                            query = query.Where(a => a.Age <= maxAge);
                            break;
                        case "Location":
                            // Find the location that matches the input
                            string loc = filter[f].Substring(col + 1);
                            string[] locArr = loc.Split('|');

                            query = query.Where(e => locArr.Contains(e.Area));
                            break;

                        case "Gender":
                            // Find the location that matches the input
                            string gen = filter[f].Substring(col + 1);
                            string[] genArr = gen.Split('|');
                                
                            query = query.Where(e => genArr.Contains(e.Gender));
                            break;

                        case "Vaccinated":
                            // Find the location that matches the input
                            string vac = filter[f].Substring(col + 1);
                            string[] vacArr = vac.Split('|');

                            query = query.Where(e => vacArr.Contains(e.Vaccinated));
                            break;

                    }
                }
                query = query.GroupBy(p => p.Reg_No).Select(g => g.FirstOrDefault());

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, query.ToList());
            }
        }

        private string[] CodeCollect(string[] arr)
        {
            ArrayList arrayList = new ArrayList();
            foreach (string p in arr)
            {
                if (p.Equals("A1"))
                {
                    arrayList.Add("A1");
                    arrayList.Add("A");
                    continue;
                }
                if (p.Equals("B"))
                {
                    arrayList.Add("B");
                    arrayList.Add("EB");
                    arrayList.Add("C1");
                    arrayList.Add("C");
                    arrayList.Add("EC1");
                    arrayList.Add("EC");
                    continue;
                }
                if (p.Equals("EB"))
                {
                    arrayList.Add("EB");
                    arrayList.Add("EC1");
                    arrayList.Add("EC");
                    continue;
                }
                if (p.Equals("C1"))
                {
                    arrayList.Add("C1");
                    arrayList.Add("C");
                    arrayList.Add("EC1");
                    arrayList.Add("EC");
                    continue;
                }
                if (p.Equals("C"))
                {
                    arrayList.Add("C");
                    arrayList.Add("EC");
                    continue;
                }
                if (p.Equals("EC1"))
                {
                    arrayList.Add("EC1");
                    arrayList.Add("EC");
                    continue;
                }
                if (p.Equals("F1"))
                {
                    arrayList.Add("F1");
                    arrayList.Add("F2");
                    arrayList.Add("F3");
                    arrayList.Add("F4");
                    continue;
                }
                if (p.Equals("F2"))
                {
                    arrayList.Add("F2");
                    arrayList.Add("F3");
                    arrayList.Add("F4");
                    continue;
                }
                if (p.Equals("F3"))
                {
                    arrayList.Add("F3");
                    arrayList.Add("F4");
                    continue;
                }
                if (p.Equals("F6"))
                {
                    arrayList.Add("F6");
                    arrayList.Add("F7");
                    continue;
                }
                if (p.Equals("F8"))
                {
                    arrayList.Add("F8");
                    arrayList.Add("F9");
                    continue;
                }
                if (p.Equals("F13"))
                {
                    arrayList.Add("F13");
                    arrayList.Add("F14");
                    continue;
                }
                if (p.Equals("F15"))
                {
                    arrayList.Add("F15");
                    arrayList.Add("F16");
                    continue;
                }
                if (p.Equals("C33"))
                {
                    arrayList.Add("C33");
                    arrayList.Add("C35");
                    continue;
                }
                if (p.Equals("C34"))
                {
                    arrayList.Add("C34");
                    arrayList.Add("C36");
                    continue;
                }
                if (p.Equals("C37"))
                {
                    arrayList.Add("C37");
                    arrayList.Add("C39");
                    continue;
                }
                if (p.Equals("C38"))
                {
                    arrayList.Add("C38");
                    arrayList.Add("C40");
                    continue;
                }
                if (p.Equals("C43"))
                {
                    arrayList.Add("C43");
                    arrayList.Add("C33");
                    arrayList.Add("C35");
                    continue;
                }
                if (p.Equals("Goods"))
                {
                    arrayList.Add("Goods");
                    continue;
                }
                if (p.Equals("Passengers"))
                {
                    arrayList.Add("Passengers");
                    continue;
                }
                if(p.Equals("Dangerous Goods"))
                {
                    arrayList.Add("Dangerous Goods");
                    continue;
                }
            }

            string[] arr2 = arrayList.ToArray(typeof(string)) as string[];
            return arr2;
        }

        /// <summary>
        /// Get the Stats for the Home Page
        /// </summary>
        /// <returns></returns>
        [Route("api/Recruitment/GetStats")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetStats()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                ArrayList dateArr = new ArrayList();
                ArrayList arr = new ArrayList();
                for(var day = 1; day < 6; day++)
                {
                    var date = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + day);
                    dateArr.Add(date);

                    var Candidates_Applied = (from rt in entities.RecruitmentTable
                               where rt.Date_Added == date
                               select rt).Count();

                    var Candidates_Processed = (from rt in entities.RecruitmentTable
                                       where rt.Date_Added == date && rt.Initial_Impression.Equals("Approved")
                                       select rt).Count();

                    var Vetting_Complete = (from rt in entities.RecruitmentTable
                                   where rt.Date_Added == date && rt.Vetting_Finished == true
                                   select rt).Count();

                    var Vetting_Incomplete = (from rt in entities.RecruitmentTable
                                    where rt.Date_Added == date && rt.Initial_Impression.Equals("Approved") && rt.Vetting_Finished == false
                                    select rt).Count();

                    var Waiting_Approval = (from rt in entities.RecruitmentTable
                                        where rt.Date_Added == date && rt.ABET_Lit > 39 && rt.ABET_Num > 39 && rt.Interview > 5 &&
                                        rt.Reference_1 > 5 && rt.Reference_2 > 5 && rt.Crime_Check.Equals("Clear") && rt.Master_Contract == true && String.IsNullOrEmpty(rt.Manager_Approval)
                                        select rt).Count();

                    var Approval_Declined = (from rt in entities.RecruitmentTable
                                            where rt.Date_Added == date && rt.Vetting_Finished == true && rt.Manager_Approval.Equals("Decline")
                                            select rt).Count();

                    arr.Add(new { date, Candidates_Applied, Candidates_Processed, Vetting_Complete, Vetting_Incomplete, Waiting_Approval, Approval_Declined });
                }

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, arr);
            }
        }

        /// <summary>
        /// Get Stat for Previous Period
        /// </summary>
        /// <returns></returns>
        [Route("api/Recruitment/GetStatsPrevious")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetStatsPrevious()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                ArrayList dateArr = new ArrayList();
                ArrayList arr = new ArrayList();
                
                var date = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
                dateArr.Add(date);

                var Candidates_Applied = (from rt in entities.RecruitmentTable
                                            where rt.Date_Added < date
                                            select rt).Count();

                var Candidates_Processed = (from rt in entities.RecruitmentTable
                                            where rt.Date_Added < date && rt.Initial_Impression.Equals("Approved")
                                            select rt).Count();

                var Vetting_Complete = (from rt in entities.RecruitmentTable
                                        where rt.Date_Added < date && rt.Initial_Impression.Equals("Approved") && rt.Vetting_Finished == true
                                        select rt).Count();

                var Vetting_Incomplete = (from rt in entities.RecruitmentTable
                                            where rt.Date_Added < date && rt.Initial_Impression.Equals("Approved") && rt.Vetting_Finished == false
                                            select rt).Count();

                var Waiting_Approval = (from rt in entities.RecruitmentTable
                                        where rt.Date_Added < date && rt.ABET_Lit > 39 && rt.ABET_Num > 39 && rt.Interview > 5 &&
                                        rt.Reference_1 > 5 && rt.Reference_2 > 5 && rt.Crime_Check.Equals("Clear") && rt.Master_Contract == true && String.IsNullOrEmpty(rt.Manager_Approval)
                                        select rt).Count();

                var Approval_Declined = (from rt in entities.RecruitmentTable
                                            where rt.Date_Added < date && rt.Vetting_Finished == true && rt.Manager_Approval.Equals("Decline")
                                            select rt).Count();

                arr.Add(new { date, Candidates_Applied, Candidates_Processed, Vetting_Complete, Vetting_Incomplete, Waiting_Approval, Approval_Declined });
                

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, arr);
            }
        }

        /// <summary>
        /// Get All the Incomplete Vetted Candidates
        /// </summary>
        /// <returns></returns>
        [Route("api/Recruitment/GetIncompleteVet")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetIncompleteVet(int? day = null)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {

                var query = (from rt in entities.RecruitmentTable
                             where rt.Initial_Impression.Equals("Approved") && rt.Vetting_Finished == false && (rt.ABET_Lit == null || rt.ABET_Num == null || rt.Interview == null ||
                             rt.Reference_1 == null || rt.Reference_2 == null || String.IsNullOrEmpty(rt.Crime_Check) || rt.Master_Contract == null)

                             join details in entities.Employee_Detail
                             on rt.Reg_No equals details.Registration_No

                             select new
                             {
                                 rt.Reg_No,
                                 rt.Name,
                                 rt.Surname,
                                 rt.ID,
                                 rt.Passport,
                                 rt.Initial_Impression,
                                 rt.ABET_Lit,
                                 rt.ABET_Num,
                                 rt.Interview,
                                 rt.Reference_1,
                                 rt.Reference_2,
                                 rt.Crime_Check,
                                 rt.Master_Contract,
                                 rt.Manager_Approval,
                                 rt.Date_Added,
                                 details.Nationality,
                                 details.Home_Language,
                                 details.Disabled,
                                 details.Martial_Status,
                                 details.Health,
                                 details.Vaccinated,
                                 details.NoK_Name,
                                 details.Own_Transport,
                                 details.Accident_History
                             });

                if(day != null && day != 0 && day != 7)
                {
                    var date = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)day);
                    query = query.Where(e => e.Date_Added == date);
                }else if(day != null && day == 7)
                {
                    var date = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
                    Debug.WriteLine(date);
                    query = query.Where(e => e.Date_Added >= date);
                }
                else
                {
                    var date = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
                    Debug.WriteLine(date);
                    query = query.Where(e => e.Date_Added < date);
                }
                

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, query.ToList());
            }
        }
    }
}
