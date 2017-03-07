using Zeta.TreeSharp;

namespace QuestTools.Helpers
{
    public static class Common
    {
        public delegate bool BoolDelegate(object ret);
        public delegate void VoidDelegate(object ret);
        public delegate object ObjectDelegate(object ret);
        public delegate Composite CompositeDelegate(object ret);
        public delegate Composite LinkCompositeDelegate();

        public delegate Composite CreateBehavior(object ret);

        //Condition Failure => return Success
        //Behavior Failure => return Success
        //Behavior Success => return Success
        public static Composite ExecuteReturnAlwaysSuccess(BoolDelegate condition, CreateBehavior behavior)
        {
            return
            new DecoratorContinue(ret => condition.Invoke(null),
                new PrioritySelector(
                    behavior.Invoke(null),
                    new Zeta.TreeSharp.Action(ret => RunStatus.Success)
                )
            );
        }

        //Condition Failure => return Failure
        //Behavior Failure => return Failure
        //Behavior Success => return Success
        public static Composite ExecuteReturnFailureOrBehaviorResult(BoolDelegate condition, CreateBehavior behavior)
        {
            return new Decorator(ret => condition.Invoke(null), behavior.Invoke(null));
        }

        //Condition Failure => return Success
        //Behavior Failure => return Failure
        //Behavior Success =>return Success
        public static Composite ExecuteReturnSuccessOrBehaviorResult(BoolDelegate condition, CreateBehavior behavior)
        {
            return new DecoratorContinue(ret => condition.Invoke(null), behavior.Invoke(null));
        }


    }
}
