using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubble.model
{
    [Serializable]
    class Setting
    {
        /// <summary>
        /// the position point x of slider window
        /// </summary>
        [JsonProperty]
        public static int sliderX = 0;
        /// <summary>
        /// the position point y of slider window
        /// </summary>
        [JsonProperty]
        public static int sliderY = 0;
        /// <summary>
        /// the cost time of comment shade in in slider window
        /// </summary>
        [JsonProperty]
        public static int shadeInTime = 500;
        /// <summary>
        /// the cost time of comment keep in slider window 
        /// </summary>
        [JsonProperty]
        public static int existTime = 5000;
        /// <summary>
        /// the cost time of comment shade out in slider window
        /// </summary>
        [JsonProperty]
        public static int shadeOutTime = 500;
        /// <summary>
        /// the font size of comment in slider window
        /// </summary>
        [JsonProperty]
        public static int sliderFontSize = 15;
        /// <summary>
        /// will the slider window show the welcome message of vip
        /// </summary>
        [JsonProperty]
        public static bool isShownWelcome = true;
        /// <summary>
        /// will the slider windows show the gift message of audience
        /// </summary>
        [JsonProperty]
        public static bool isShownGift = true;
    }
}
