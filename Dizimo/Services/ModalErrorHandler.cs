namespace Dizimo.Services
{
    /// <summary>
    /// Modal Error Handler com logging
    /// </summary>
    public class ModalErrorHandler : IErrorHandler
    {
        private readonly IDialogService _dialogService;
        private SemaphoreSlim _semaphore = new(1, 1);

        public ModalErrorHandler(IDialogService? dialogService = null)
        {
            _dialogService = dialogService ?? new DialogService();
        }

        /// <summary>
        /// Handle error in UI com logging.
        /// </summary>
        /// <param name="ex">Exception.</param>
        public void HandleError(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] {ex.Message}");
            DisplayErrorAsync(ex).FireAndForgetSafeAsync();
        }

        private async Task DisplayErrorAsync(Exception ex)
        {
            try
            {
                await _semaphore.WaitAsync();
                await _dialogService.ShowErrorAsync(ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}