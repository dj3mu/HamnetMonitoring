using System;
using Newtonsoft.Json;
using RestService.DataFetchingService;

namespace HamnetDbRest
{
    internal class SingleFailureInfoWithEntity : ISingleFailureInfoWithEntity
    {
        private readonly EntityType entityType;

        private readonly string entity;

        private readonly ISingleFailureInfo penaltyInfo;

        /// <summary>
        /// Initialize for the given type and entity string with the given penalty info.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        /// <param name="entity">The name of the entity (e.g. an IP address or a subnet in CIDR).</param>
        /// <param name="penaltyInfo">The associated penalty info.</param>
        public SingleFailureInfoWithEntity(EntityType entityType, string entity, ISingleFailureInfo penaltyInfo)
        {
            this.entityType = entityType;
            this.entity = entity ?? string.Empty;
            this.penaltyInfo = penaltyInfo ?? throw new ArgumentNullException(nameof(penaltyInfo), "The penalty info is null");
        }

        /// <inheritdoc />
        [JsonProperty(Order = 26)]
        public string Entity => this.entity;

        /// <inheritdoc />
        [JsonProperty(Order = 25)]
        public EntityType EntityType => this.entityType;

        /// <inheritdoc />
        [JsonProperty(Order = 1)]
        public uint OccuranceCount => this.penaltyInfo.OccuranceCount;

        /// <inheritdoc />
        [JsonProperty(Order = 3)]
        public DateTime LastOccurance => this.penaltyInfo.LastOccurance;

        /// <inheritdoc />
        [JsonProperty(Order = 2)]
        public DateTime FirsOccurance => this.penaltyInfo.FirsOccurance;

        /// <inheritdoc />
        [JsonProperty(Order = 4)]
        public TimeSpan CurrentPenalty => this.penaltyInfo.CurrentPenalty;

        /// <inheritdoc />
        [JsonProperty(Order = 5)]
        public bool IsRetryFeasible
        {
            get
            {
                var retryFeasible = ((DateTime.UtcNow - this.LastOccurance) > this.CurrentPenalty);
                return retryFeasible;
            }

            set
            {
                // NOP - we ignore the stored value and always recompute in getter
            }
        }
    }
}
