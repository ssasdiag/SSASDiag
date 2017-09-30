namespace SimpleMDXParser
{
    using System;

    public class CubeInfo
    {
        public virtual MDXDataType DetermineType(string id)
        {
            if (!id.Equals("Measures", StringComparison.InvariantCultureIgnoreCase) && !id.Equals("[Measures]", StringComparison.InvariantCultureIgnoreCase))
            {
                return MDXDataType.Member;
            }
            return MDXDataType.Hierarchy;
        }
    }
}

