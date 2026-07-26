namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    // Used by the Error page to display error details
    public class ErrorViewModel
    {
        public ErrorViewModel(string? requestId)
        {
            RequestId = requestId;
        }

        public string? RequestId { get; set; }


        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
