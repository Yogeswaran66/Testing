using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace exhelper
{
    public static class PathHelper
    {
        public static String FixPath(String DirPath, String RefDir)
        {
            String RefSuffix = String.Empty;

            try
            {
                DirPath = DirPath.ToUpper().Trim();

                // handle simple cases first
                if (Regex.IsMatch(DirPath, @"[A-Z]:\*"))
                {
                    return DirPath;
                }

                if (DirPath.Substring(DirPath.Length - 1, 1) == @"\")
                {
                    DirPath = DirPath.Substring(0, DirPath.Length - 1);
                }

                if (Regex.IsMatch(DirPath, @"[A-Z]:\*"))
                {
                    return DirPath;
                }

                if (String.IsNullOrEmpty(RefDir))
                {
                    RefDir = Path.GetPathRoot(Environment.CurrentDirectory);
                }
                else
                {
                    RefDir = RefDir.Trim();
                    if (String.IsNullOrEmpty(RefDir))
                    {
                        RefDir = Path.GetPathRoot(Environment.CurrentDirectory);
                    }
                }


                RefDir = RefDir.ToUpper();

                if (Regex.IsMatch(RefDir, @"[A-Z]:\*"))
                {
                    if (!Regex.IsMatch(RefDir, @"[A-Z]:\"))
                    {
                        if (RefDir.Substring(RefDir.Length - 1, 1) == @"\")
                        {
                            RefDir = RefDir.Substring(0, RefDir.Length - 1);
                        }

                        RefSuffix = @"\";
                    }
                }
                else
                {
                    if (RefDir.Substring(RefDir.Length - 1, 1) == @"\")
                    {
                        RefDir = RefDir.Substring(0, RefDir.Length - 1);
                    }

                    RefSuffix = @"\";
                }

                switch (DirPath)
                {
                    case ".":
                    case @".\":
                        return RefDir;
                    case @"\":
                        return RefDir.Substring(0, 3);
                    case "..":
                    case @"..\":
                        return Directory.GetParent(RefDir).FullName;
                }

                if (Regex.IsMatch(DirPath, @".\*"))
                {
                    return RefDir + RefSuffix + DirPath.Substring(3);
                }

                while (true)
                {
                    if (DirPath.Substring(0, 3) == @"..\")
                    {
                        RefDir = Directory.GetParent(RefDir).FullName;

                        if (String.IsNullOrEmpty(RefDir))
                        {
                            return String.Empty;
                        }

                        DirPath = DirPath.Substring(4);
                    }
                    else
                    {
                        if (RefDir.Substring(RefDir.Length - 1, 1) == @"\")
                        {
                            return RefDir + DirPath;
                        }
                        else
                        {
                            return RefDir + @"\" + DirPath;
                        }
                    }

                    if (String.IsNullOrEmpty(DirPath) || DirPath.Length == 0)
                    {
                        return String.Empty;
                    }
                }
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

    }
}
