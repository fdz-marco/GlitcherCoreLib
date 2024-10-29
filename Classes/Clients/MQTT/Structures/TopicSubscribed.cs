namespace glitcher.core.Clients
{
    /// <summary>
    /// (Class/Object Definition) MQTT Topic Subscribed
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.10.25 - October 25, 2024
    /// </remarks>

    public class TopicSubscribed
    {

        #region Properties

        public string path { get; set; }
        public string payload { get; set; }
        public DateTime subscribedOn { get; set; }
        public DateTime updatedOn { get; set; }

        #endregion

        #region Constructor

        public TopicSubscribed(string path, string payload)
        {
            this.path = path;
            this.payload = payload;
            this.subscribedOn = DateTime.Now;
            this.updatedOn = DateTime.Now;
        }

        #endregion

        #region Methods

        public void Update(string payload)
        {
            this.payload = payload;
            this.updatedOn = DateTime.Now;
        }

        #endregion
    }
}