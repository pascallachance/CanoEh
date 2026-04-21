namespace Infrastructure.Repositories.Exceptions
{
    public class DuplicateItemReviewException : Exception
    {
        public DuplicateItemReviewException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
