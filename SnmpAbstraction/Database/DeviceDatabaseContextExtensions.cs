using System;
using System.Linq;
using SemVersion;

namespace SnmpAbstraction
{
    internal static class DeviceDatabaseContextExtensions
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Tries to obtain the device ID (database key) for the device of the given name.
        /// </summary>
        /// <param name="context">The device database context to extend.</param>
        /// <param name="deviceName">The device name to search for.</param>
        /// <param name="deviceId">Returns the database key, if the device of given name has been found.</param>
        /// <returns><c>true</c> if a device of the given name has been found and the ID returned. Otherwise <c>false</c>.</returns>
        public static bool TryFindDeviceId(this DeviceDatabaseContext context, string deviceName, out int deviceId)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "The context to search device ID on is null");
            }

            if (String.IsNullOrWhiteSpace(deviceName))
            {
                throw new ArgumentNullException(nameof(deviceName), "The device name to search device ID for is null, empty or white-space-only");
            }

            deviceId = -1;

            Device result = context.Devices.FirstOrDefault(d => d.Name == deviceName);

            if (result == null)
            {
                return false;
            }

            deviceId = result.Id;

            return true;
        }

        /// <summary>
        /// Tries to obtain the device version ID (database key) for the given device version (or the highest available version if null).
        /// </summary>
        /// <param name="context">The device database context to extend.</param>
        /// <param name="deviceId">The device ID to search versions for.</param>
        /// <param name="version">The version number to return the device version ID for. If null, the ID for the highest available version will be retrieved.</param>
        /// <param name="deviceVersionId">Returns the database key, if a device version of for given device ID and version range has been found.</param>
        /// <returns><c>true</c> if a device version of the given version has been found and the ID returned. Otherwise <c>false</c>.</returns>
        public static bool TryFindDeviceVersionId(this DeviceDatabaseContext context, int deviceId, SemanticVersion version, out int deviceVersionId)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "The context to search device version ID on is null");
            }

            deviceVersionId = -1;

            var result = context.DeviceVersions.Where(d => d.DeviceId == deviceId).OrderByDescending(d => d.MinimumVersion).ToList();

            if (result.Count == 0)
            {
                // no such device ID at all
                return false;
            }

            if (version == null)
            {
                // no specific version requested, returns first entry (i.e. the one with the highest MinimumVersion)
                deviceVersionId = result[0].Id;
                return true;
            }

            // need to search for the best match of specified version:
            // we take the entry with the highest minimum version where the requested version is below maximum version
            foreach (DeviceVersion item in result)
            {
                if ((item.MinimumVersion <= version) && (item.MaximumVersion >= version))
                {
                    deviceVersionId = item.Id;
                    return true;
                }
            }

            log.Debug($"TryFindDeviceVersionId: No version range for device ID {deviceId} in database is applicable to requested version '{version}'");

            return false;
        }

        /// <summary>
        /// Tries to obtain the OID lookup ID (database key) for the device version ID.
        /// </summary>
        /// <param name="context">The device database context to extend.</param>
        /// <param name="deviceVersionId">The device version ID to look up the mapping ID for.</param>
        /// <param name="oidMappingId">Returns the mapping ID, if found.</param>
        /// <returns><c>true</c> if a lookup of the given ID has been found and returned. Otherwise <c>false</c>.</returns>
        public static bool TryFindOidLookupId(this DeviceDatabaseContext context, int deviceVersionId, out int oidMappingId)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "The context to search OID lookup ID on is null");
            }

            oidMappingId = -1;

            var result = context.DeviceVersionMapping.FirstOrDefault(d => d.DeviceVersionId == deviceVersionId);

            if (result == null)
            {
                return false;
            }

            oidMappingId = result.OidMappingId;

            return true;
        }

        /// <summary>
        /// Tries to obtain the oid lookup table given lookup ID.
        /// </summary>
        /// <param name="context">The device database context to extend.</param>
        /// <param name="oidLookupId">The OID mapping lookup ID to get.</param>
        /// <param name="oidLookup">Returns the OID lookup, if found.</param>
        /// <returns><c>true</c> if a lookup of the given lookup ID has been found and the ID returned. Otherwise <c>false</c>.</returns>
        public static bool TryFindDeviceSpecificOidLookup(this DeviceDatabaseContext context, int oidLookupId, out DeviceSpecificOidLookup oidLookup)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "The context to search OID lookup is null");
            }

            oidLookup = null;

            var result = context.DeviceSpecificOids.Where(d => d.OidMappingId == oidLookupId);

            if (!result.Any())
            {
                return false;
            }

            oidLookup = new DeviceSpecificOidLookup(result);

            return true;
        }

        /// <summary>
        /// Verifies the consistency of the database and the model (as we currently do NOT create the DB from the model).<br/>
        /// Mainly checks whether there are DB entried for each enum and vice versa.
        /// </summary>
        /// <param name="context">The device database context to extend.</param>
        /// <returns><c>true</c> if the model is consistend. Otherwise <c>false</c>. Details can be found as warning in log file.</returns>
        public static bool FullConsistencyCheck(this DeviceDatabaseContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "The context to run consistency check on is null");
            }

            return context.DataTypeConsistencyCheck()
                && context.RetrievableValueConsistencyCheck();
        }

        /// <summary>
        /// Verifies the consistency of the database and the model for DataType enum.<br/>
        /// </summary>
        /// <param name="context">The device database context to extend.</param>
        /// <returns><c>true</c> if the model is consistend. Otherwise <c>false</c>. Details can be found as warning in log file.</returns>
        public static bool DataTypeConsistencyCheck(this DeviceDatabaseContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "The context to run data type consistency check on is null");
            }

            var databaseTypes = context.DataTypes.ToList();

            bool returnValue = true;

            // check if we have an enum for each database entry
            foreach (DataType item in databaseTypes)
            {
                DataTypesEnum parsedEnum;
                bool parseSuccessful = Enum.TryParse<DataTypesEnum>(item.TypeName, out parsedEnum);
                if (!parseSuccessful)
                {
                    returnValue = false;
                    log.Error($"Database entry data type name '{item.TypeName}' cannot be found in C# enum {typeof(DataTypesEnum).FullName}");
                }

                bool isDefined = Enum.IsDefined(typeof(DataTypesEnum), item.Id);
                if (!isDefined)
                {
                    returnValue = false;
                    log.Error($"Database entry data type ID '{item.Id}' doesn't have a correponding C# enum value in {typeof(DataTypesEnum).FullName}");
                }

                if (parseSuccessful && isDefined && ((int)parsedEnum != item.Id))
                {
                    // Note: It doesn't make sense to do this check if we anyway found NO ENUM AT ALL which matches the TypeName from database
                    //       or if we didn't find any with the given integer value:
                    //       It would just produce redundant error output (it must fail)
                    returnValue = false;
                    log.Error($"Database entry data type ID '{item.Id}' of TypeName '{item.TypeName}' doesn't match C# enum of {typeof(DataTypesEnum).FullName} with same integer value. Enum name instead is '{parsedEnum}'");
                }
            }

            // check if we have a database entry for every enum value
            foreach (DataTypesEnum item in Enum.GetValues(typeof(DataTypesEnum)))
            {
                DataType idMatchingDataType = databaseTypes.FirstOrDefault(dt => dt.Id == (int)item);
                if (idMatchingDataType == null)
                {
                    returnValue = false;
                    log.Error($"C# enum value '{item}' of {typeof(DataTypesEnum).FullName} does NOT have a matching database entry with same numeric ID of {(int)item}");
                }

                DataType nameMatchingDataType = databaseTypes.FirstOrDefault(dt => dt.TypeName.Equals(item.ToString()));
                if (nameMatchingDataType == null)
                {
                    returnValue = false;
                    log.Error($"C# enum value '{item}' of {typeof(DataTypesEnum).FullName} does NOT have a matching database entry with same name");
                }

                if ((idMatchingDataType != null) && (nameMatchingDataType != null) && (idMatchingDataType != nameMatchingDataType))
                {
                    // Note: It doesn't make sense to do this check if we anyway found NO DB ENTRY AT ALL which matches the TypeName from enum
                    //       or if we didn't find any with the given integer value:
                    //       It would just produce redundant error output (it must fail)
                    returnValue = false;
                    log.Error($"C# enum value '{item}' of {typeof(DataTypesEnum).FullName} does NOT have consistent matching database entries. Database entry of same name is '{nameMatchingDataType}' while entry of same ID is '{idMatchingDataType}'");
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Verifies the consistency of the database and the model for DataType enum.<br/>
        /// </summary>
        /// <param name="context">The device database context to extend.</param>
        /// <returns><c>true</c> if the model is consistend. Otherwise <c>false</c>. Details can be found as warning in log file.</returns>
        public static bool RetrievableValueConsistencyCheck(this DeviceDatabaseContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "The context to run retrievable value consistency check on is null");
            }

            var retrievableValues = context.RetrievalValues.ToList();

            bool returnValue = true;

            // check if we have an enum for each database entry
            foreach (RetrievableValue item in retrievableValues)
            {
                RetrievableValuesEnum parsedEnum;
                bool parseSuccessful = Enum.TryParse<RetrievableValuesEnum>(item.ValueMeaning, out parsedEnum);
                if (!parseSuccessful)
                {
                    returnValue = false;
                    log.Error($"Database entry for value meaning name '{item.ValueMeaning}' cannot be found in C# enum {typeof(RetrievableValuesEnum).FullName}");
                }

                bool isDefined = Enum.IsDefined(typeof(RetrievableValuesEnum), item.Id);
                if (!isDefined)
                {
                    returnValue = false;
                    log.Error($"Database entry value meaning ID '{item.Id}' doesn't have a correponding C# enum value in {typeof(RetrievableValuesEnum).FullName}");
                }

                if (parseSuccessful && isDefined && ((int)parsedEnum != item.Id))
                {
                    // Note: It doesn't make sense to do this check if we anyway found NO ENUM AT ALL which matches the ValueMeaning from database
                    //       or if we didn't find any with the given integer value:
                    //       It would just produce redundant error output (it must fail)
                    returnValue = false;
                    log.Error($"Database entry for value meaning ID '{item.Id}' of MeaningName '{item.ValueMeaning}' doesn't match C# enum of {typeof(RetrievableValuesEnum).FullName} with same integer value. Enum name instead is '{parsedEnum}'");
                }
            }

            // check if we have a database entry for every enum value
            foreach (RetrievableValuesEnum item in Enum.GetValues(typeof(RetrievableValuesEnum)))
            {
                RetrievableValue idMatchingDataType = retrievableValues.FirstOrDefault(dt => dt.Id == (int)item);
                if (idMatchingDataType == null)
                {
                    returnValue = false;
                    log.Error($"C# enum value '{item}' of {typeof(RetrievableValuesEnum).FullName} does NOT have a matching database entry with same numeric ID of {(int)item}");
                }

                RetrievableValue nameMatchingDataType = retrievableValues.FirstOrDefault(dt => dt.ValueMeaning.Equals(item.ToString()));
                if (nameMatchingDataType == null)
                {
                    returnValue = false;
                    log.Error($"C# enum value '{item}' of {typeof(RetrievableValuesEnum).FullName} does NOT have a matching database entry with same name");
                }

                if ((idMatchingDataType != null) && (nameMatchingDataType != null) && (idMatchingDataType != nameMatchingDataType))
                {
                    // Note: It doesn't make sense to do this check if we anyway found NO DB ENTRY AT ALL which matches the TypeName from enum
                    //       or if we didn't find any with the given integer value:
                    //       It would just produce redundant error output (it must fail)
                    returnValue = false;
                    log.Error($"C# enum value '{item}' of {typeof(RetrievableValuesEnum).FullName} does NOT have consistent matching database entries. Database entry of same name is '{nameMatchingDataType}' while entry of same ID is '{idMatchingDataType}'");
                }
            }

            return returnValue;
        }
    }
}
