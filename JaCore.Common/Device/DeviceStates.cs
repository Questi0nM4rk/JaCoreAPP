namespace JaCore.Common.Device;

/// <summary>
/// Represents the current state of device data within the application.
/// Used to determine which operations can be performed on the device record.
/// </summary>
public enum DeviceDataState
{
    /// <summary>
    /// Device data is in its default state, loaded but not being modified.
    /// All read operations are valid; edit operations require transitioning to Modified state.
    /// </summary>
    Idle = 0,
    
    /// <summary>
    /// Device has been newly created but not yet saved to the database.
    /// Has a temporary ID assigned but will receive permanent ID on first save.
    /// </summary>
    New = 1,
    
    /// <summary>
    /// Device data has been modified from its original state.
    /// Changes exist in memory but have not been committed to database.
    /// </summary>
    Modified = 2,
    
    /// <summary>
    /// Device data is currently being saved to the database.
    /// No modifications should be allowed during this state.
    /// </summary>
    Saving = 3,
    
    /// <summary>
    /// Device data is locked for editing, typically because it's 
    /// currently in use in an active production process.
    /// </summary>
    Locked = 4,
    
    /// <summary>
    /// Device failed validation and contains invalid data.
    /// Must be corrected before saving is permitted.
    /// </summary>
    Invalid = 5,
    
    /// <summary>
    /// Device is being synced with an external system.
    /// Modifications should be prevented until sync completes.
    /// </summary>
    Syncing = 6,
    
    /// <summary>
    /// Device has been marked for deletion but not yet removed from the database.
    /// Allows for "soft delete" with option to recover.
    /// </summary>
    PendingDeletion = 7
}

/// <summary>
/// Represents the operational state of a physical device in the manufacturing environment.
/// Used for monitoring, reporting, and resource allocation.
/// </summary>
public enum DeviceOperationalState
{
    /// <summary>
    /// Device status is unknown or has not been determined.
    /// Default state for newly registered devices.
    /// </summary>
    Unknown = 0,
    
    /// <summary>
    /// Device is currently active and in use (within the last 30 minutes).
    /// Operators are actively working with this device.
    /// </summary>
    Active = 1,
    
    /// <summary>
    /// Device is powered on and operational but not actively being used.
    /// Available for immediate allocation to production tasks.
    /// </summary>
    Idle = 2,
    
    /// <summary>
    /// Device is allocated to and being used in an active production process.
    /// Cannot be reassigned until production completes or is paused.
    /// </summary>
    InProduction = 3,
    
    /// <summary>
    /// Device is undergoing scheduled maintenance or service.
    /// Temporarily unavailable for production use.
    /// </summary>
    InMaintenance = 4,
    
    /// <summary>
    /// Device requires maintenance or repair and cannot be used.
    /// Has been taken out of the available device pool.
    /// </summary>
    OutOfService = 5,
    
    /// <summary>
    /// Device has been calibrated and is awaiting quality control verification
    /// before returning to production.
    /// </summary>
    PendingQualityCheck = 6,
    
    /// <summary>
    /// Device is in reserve/standby mode, ready to replace another device
    /// in case of failure or for capacity scaling.
    /// </summary>
    Standby = 7,
    
    /// <summary>
    /// Device is powered off or disconnected from the system.
    /// Cannot be used without manual intervention.
    /// </summary>
    Offline = 8,
    
    /// <summary>
    /// Device is being set up for a new production run.
    /// Tools, materials, or configurations are being prepared.
    /// </summary>
    Setup = 9,
    
    /// <summary>
    /// Device is running a test or calibration cycle.
    /// Not available for production but actively measuring performance.
    /// </summary>
    Calibrating = 10
} 