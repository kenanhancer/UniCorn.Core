using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace UniCorn.Core
{
    public class ILUtility
    {
        public static Func<object[], object> CreateMethodInvokerDelegate(MethodInfo method)
        {
            var prmList = method.GetParameters().Select(f => f.ParameterType).ToList();
            prmList.Insert(0, method.DeclaringType);

            var dynamicMethod = new DynamicMethod("DynamicMethod", typeof(object), new Type[] { typeof(object[]) }, typeof(UniCornExtensions).GetTypeInfo().Module, skipVisibility: true);

            ILGenerator ilGen = dynamicMethod.GetILGenerator();

            for (int a = 0; a < prmList.Count; a++)
            {
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldc_I4, a);
                ilGen.Emit(OpCodes.Ldelem_Ref);

                ilGen.Emit(OpCodes.Unbox_Any, prmList[a]);
            }

            ilGen.Emit(OpCodes.Callvirt, method);

            ilGen.Emit(OpCodes.Box, method.ReturnType);

            ilGen.Emit(OpCodes.Ret);

            return (Func<object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object[], object>));
        }

        public static Func<object[], object> CreateInstanceDelegate(Type type)
        {
            var dynamicMethod = new DynamicMethod(type.Name, typeof(object), new Type[] { typeof(object[]) }, typeof(UniCornExtensions).GetTypeInfo().Module, skipVisibility: true);

            ILGenerator il = dynamicMethod.GetILGenerator();

            bool isConstructorEmit = EmitNewObjOpCode(il, type);

            if (!isConstructorEmit)
            {
                throw new NotSupportedException(string.Format("There is no constructor for {0}.", type.Name));
            }

            il.Emit(OpCodes.Ret);

            return (Func<object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object[], object>));
        }

        private static bool EmitNewObjOpCode(ILGenerator il, Type type)
        {
            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);

            if (constructor == null)
            {
                constructor = type.GetConstructors().FirstOrDefault();

                if (constructor != null)
                {
                    var parameters = constructor.GetParameters();
                    ParameterInfo prm;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        prm = parameters[i];

                        if (prm.ParameterType == type)
                        {
                            il.Emit(OpCodes.Ldnull);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldc_I4, i);
                            il.Emit(OpCodes.Ldelem_Ref);

                            il.Emit(OpCodes.Unbox_Any, prm.ParameterType);
                        }
                    }
                }
            }

            if (constructor != null)
            {
                il.Emit(OpCodes.Newobj, constructor);
                return true;
            }

            return false;
        }

        private static void EmitLoadConstantInt(ILGenerator il, int i)
        {
            switch (i)
            {
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    il.Emit(OpCodes.Ldc_I4, i);
                    break;
            }
        }

        private static void EmitMethodCall(MethodInfo method, ILGenerator il)
        {
            il.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);
        }
    }
}