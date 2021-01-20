using System;

namespace crds_angular.Exceptions
{
    public class ContactEmailExistsException : Exception
    {
        private readonly int contactId;
        private readonly string email;

        public ContactEmailExistsException(int contactId, string email) : base(string.Format("Contact ({0}) with email {1} already exists", contactId, email))
        {
            this.contactId = contactId;
            this.email = email;
        }

        public int ContactId()
        {
            return contactId;
        }

        public string Email()
        {
            return email;
        }
    }
}