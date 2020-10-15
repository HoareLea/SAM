namespace SAM.Core.Grasshopper
{
    public interface IGH_SAMComponent
    {
        string ComponentVersion { get;  }

        string SAMVersion { get;  }

        string LatestComponentVersion { get;  }
    }
}
