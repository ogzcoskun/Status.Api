namespace Status.Library
{
    public class Library : ILibrary
    {
        public async Task<string> GetFormattedTime()
        {
            var time = (Convert.ToDateTime(DateTime.Now.ToString("MM/dd/yyyy HH:mm"))).ToString();
            return time;
        }
    }
}
