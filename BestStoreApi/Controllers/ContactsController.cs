using BestStoreApi.Models;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BestStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        #region dbcontext dependency injection
        private readonly ApplicationDbContext _context;
        private readonly EmailSender _emailSender; 
        public ContactsController(ApplicationDbContext context, EmailSender emailSender)
        {
            this._context = context;
            this._emailSender = emailSender;
        }
        #endregion

        #region DropDown

        [HttpGet("subjects")]
        public IActionResult GetSubjects()
        {
            var listSubjects = _context.Subjects.ToList();
            return Ok(listSubjects);
        }

        #endregion

        #region GetAll and GetByID

        [HttpGet]
        public IActionResult GetContacts(int? page)
        {
            if (page == null || page < 1)
            {
                page = 1; 
            }
            
            int pageSize = 5;
            int totalPages = 0;

            decimal count = _context.Contacts.Count();

            totalPages = (int) Math.Ceiling(count/pageSize);

            var contacts = _context.Contacts
                .Include(c => c.Subject)
                .OrderBy(c => c.Id)
                .Skip((int) (page - 1) * pageSize )
                .Take(pageSize)
                .ToList();

            var response = new
            {
                Contacts = contacts,
                TotalPages = totalPages,
                PageSize = pageSize,
                Page = page
            };

            return Ok(response);
        }

        [HttpGet("{Id}")]
        public IActionResult GetContactsById(int Id)
        {
            var contact = _context.Contacts.Include(c => c.Subject).FirstOrDefault(c => c.Id == Id);
            if (contact == null)
            {
                return NotFound();
            }
            return Ok(contact);
        }

        #endregion

        #region CreateContact

        [HttpPost]
        public IActionResult CreateContact(ContactDto contactDto)
        {
            var subject = _context.Subjects.Find(contactDto.SubjectId);
            if(subject is null)
            {
                ModelState.AddModelError("Subject", "Pkease select a valid subject");
                return BadRequest(ModelState);
            }

            Contacts contact = new Contacts()
            {
                FirstName = contactDto.FirstName,
                LastName = contactDto.LastName,
                Email = contactDto.Email,
                Phone = contactDto.Phone ?? "",
                Subject = subject,
                Message = contactDto.Message,
                CreatedAt = DateTime.Now
            };
            _context.Contacts.Add(contact);
            _context.SaveChanges();

            // send email
            string emailSubject = "Confirmation Email";
            string userName = contactDto.FirstName + " " + contactDto.LastName;
            string emailMessage = "Dear " + userName + "\n" +
                "We received your message. Thank you for contacting us.\n" +
                "Our team will contact you very soon.\n" +
                "Best Regards";

            _emailSender.SendEmail(emailSubject, contact.Email, userName, emailMessage).Wait();

            return Ok(contact);
        }

        #endregion

        #region Update

        [HttpPut("{Id}")]
        public IActionResult UpdateContact(int Id, ContactDto contactDto)
        {
            var subject = _context.Subjects.Find(contactDto.SubjectId);
            if (subject is null)
            {
                ModelState.AddModelError("Subject", "Pkease select a valid subject");
                return BadRequest(ModelState);
            }

            var contact = _context.Contacts.Find(Id);
            if (contact == null)
            {
                return NotFound();
            }
            contact.FirstName = contactDto.FirstName;
            contact.LastName = contactDto.LastName;
            contact.Email = contactDto.Email;
            contact.Phone = contactDto.Phone ?? "";
            contact.Subject = subject;
            contact.Message = contactDto.Message;

            _context.SaveChanges();
            return Ok();
        }

        #endregion

        #region Delete

        [HttpDelete("{Id}")]
        public IActionResult DeleteContact(int Id)
        {
            try
            {
                var contact = new Contacts() { Id = Id , Subject = new Subject() };
                _context.Contacts.Remove(contact);
                _context.SaveChanges();
            }
            catch (Exception)
            {
                return NotFound();
            }

            return Ok();
        }
        #endregion
    }
}
