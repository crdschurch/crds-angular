using System;

namespace crds_angular.Exceptions
{
    public class ChildcareDatesMissingException : Exception
    {
        public int ChildcareRequestId { get; set; }

        public ChildcareDatesMissingException(int childcareRequest) : base(string.Format("Childcare Request {0} has no dates associated with it.", childcareRequest))
        {
            this.ChildcareRequestId = childcareRequest;
        }
    }
}