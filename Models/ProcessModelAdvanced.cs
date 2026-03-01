public class ProcessAdvancedModel
{
    public int ProcessID { get; set; } // check
    public string ProcessName { get; set; }// check
    public string ProcessPath { get; set; } // check
    public DateTime ProcessStartTime { get; set;}
    public bool ProcessIsPrivilegied { get; set; } // check
    public int ProcessInstancesNumber { get; set; } // check
    public int ProcessModulesNumber { get; set; } // check
    public int ProcessThreadsNumber { get; set; } // check

    public double ProcessCpuUsage { get; set; }    // check
    public double ProcessDiskUsage { get; set; } // check
    public ProcessMemoryData processMemoryData { get; set; } // check
    
} 

public class ProcessMemoryData
{
    public double ProcessMemoryPhisyckUsage { get; set; }
    public double ProcessMemoryVirtualUsage { get; set; }
    public double ProcessMemoryPagedUsage { get; set; }
    public double ProcessMemoryPrivateUsage { get; set; }
}