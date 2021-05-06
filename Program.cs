using System;
using System.Reflection;
using System.Security.Cryptography;

namespace fips_machine_verification
{
    class Program
    {
        static void Main(string[] args)
        {
            Process();
        }

        /*
         * Thanks to:
         *   http://web.archive.org/web/20160111071952/blogs.msdn.com/b/icumove/archive/2009/01/31/working-with-fips-in-net-c.aspx
         *   https://stackoverflow.com/questions/54119906/detecting-if-fips-is-being-enforced-via-net-c-sharp
         */
        private static void Process()
        {
            bool fipsEnabled = CryptoConfig.AllowOnlyFipsAlgorithms;
            if (!fipsEnabled) 
            {
                Console.WriteLine("FIPS is not enabled on this machine");
                return;
            }

            Assembly core = Assembly.Load("System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Assembly mscorlib = Assembly.Load("mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            Type[] subclasses = new Type[]
            {
                typeof(SymmetricAlgorithm),
                typeof(HashAlgorithm),
                typeof(AsymmetricAlgorithm)
            };

            Print(mscorlib, subclasses);
            Console.WriteLine();
            Console.WriteLine();
            Print(core, subclasses);

            return;
        }

        private static void Print(Assembly asm, Type[] subclasses)
        {
            string columnFormat = "{0,-35}{1,-15}{2}";
            Console.WriteLine("FIPS Compliant in {0}", asm.GetName());
            Console.WriteLine(columnFormat, "Name", "Compliant", "Subclass");

            foreach (Type type in asm.GetTypes())
            {
                foreach (Type subclass in subclasses)
                {
                    if (type.IsSubclassOf(subclass))
                    {
                        if (!type.IsAbstract)
                        {
                            string isCompliant = null;
                            try
                            {
                                Activator.CreateInstance(type);
                                isCompliant = "Y";
                            }
                            catch (TargetInvocationException)
                            {
                                isCompliant = "N";
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                            finally
                            {
                                Console.WriteLine(columnFormat, type.Name, isCompliant, subclass.Name);
                            }
                        }
                    }
                }
            }
        }
    }
}
