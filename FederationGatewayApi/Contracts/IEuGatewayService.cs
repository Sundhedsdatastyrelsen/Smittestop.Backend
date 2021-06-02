namespace FederationGatewayApi.Contracts
{
    public interface IEuGatewayService
    {
        /// <param name="uploadKeysAgeLimitInDays">Upload keys not older then N days</param>
        /// <param name="batchSize">How many keys will be added to each batch.</param>
        /// <param name="batchCountLimit">Limit how many batches can be sent.</param>
        /// <param name="logInformationKeyValueOnUpload">If true key values are information logged</param>
        void UploadKeysToTheGateway(int uploadKeysAgeLimitInDays, int batchSize, int? batchCountLimit = null, bool logInformationKeyValueOnUpload = false);

        void DownloadKeysFromGateway(int forLastNumberOfDays);

    }
}
