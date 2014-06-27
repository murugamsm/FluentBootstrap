﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FluentBootstrap
{
    // This (and derived) non-generic interfaces get applied to every component
    // This allowed type comparisons of component references without worrying about all the generic stuff
    // Generally, any members that need to be accessed after getting a component off the stack from another one should be put in the interface
    internal interface IComponent
    {
        void Start(TextWriter writer, bool @implicit);
        void Finish(TextWriter writer);
        void StartAndFinish(TextWriter writer);
        bool Implicit { get; }
    }

    public abstract class Component : IComponent
    {
        void IComponent.Start(TextWriter writer, bool isImplicit)
        {
            Start(writer, isImplicit);
        }

        void IComponent.Finish(TextWriter writer)
        {
            Finish(writer);
        }

        void IComponent.StartAndFinish(TextWriter writer)
        {
            StartAndFinish(writer);
        }

        bool IComponent.Implicit
        {
            get { return Implicit; }
        }

        internal abstract void Start(TextWriter writer, bool @implicit);
        internal abstract void Finish(TextWriter writer);
        internal abstract void StartAndFinish(TextWriter writer);
        internal abstract bool Implicit { get; }
    }

    public abstract class Component<TModel, TThis> : Component, IDisposable, IHtmlString, IComponentCreator<TModel>
        where TThis : Component<TModel, TThis>
    {
        private bool _disposed;
        private bool _started;
        private bool _ended;
        private bool _render = true;
        private readonly List<IComponent> _children = new List<IComponent>();

        // Implicit components are created by the library as wrappers, missing tags, etc.
        // The primary difference is that implicit components can be automatically cleaned up from the stack
        private bool _implicit;

        internal override bool Implicit
        {
            get { return _implicit; }
        }

        internal BootstrapHelper<TModel> Helper { get; private set; }

        protected Component(BootstrapHelper<TModel> helper)
        {
            // Sanity check
            if (typeof(TThis) != this.GetType())
            {
                throw new Exception("Invalid TThis generic type parameter for " + this.GetType().Name + " (you should never see this).");
            }

            Helper = helper;
            PendingComponents.Add(HtmlHelper, this);
        }

        public BootstrapHelper<TModel> GetHelper()
        {
            return Helper;
        }

        internal TThis GetThis()
        {
            return (TThis)this;
        }

        internal TThis AddChild(IComponent component)
        {
            _children.Add(component);
            PendingComponents.Remove(HtmlHelper, component); // Remove the pending child component because it's now associated with this one
            return GetThis();
        }

        public void Begin()
        {
            PendingComponents.Start(HtmlHelper);
        }

        public void End()
        {
            Dispose();
        }

        // Setting this to false prevents all output
        public TThis If(bool condition)
        {
            _render = condition;
            return (TThis)this;
        }

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
            _disposed = true;
            PendingComponents.Start(HtmlHelper);
            Finish(ViewContext.Writer);
        }

        internal HtmlHelper<TModel> HtmlHelper
        {
            get { return Helper.HtmlHelper; }
        }

        internal ViewContext ViewContext
        {
            get { return HtmlHelper.ViewContext; }
        }

        internal override void Start(TextWriter writer, bool isImplicit)
        {
            // Only write content once
            if (_started)
                return;
            _started = true;

            // Mark the implicit flag
            _implicit = isImplicit;

            // Remove this component from the pending list
            PendingComponents.Remove(HtmlHelper, this);

            // Stop now if not rendering
            if (!_render)
                return;

            // Prepare this component
            PreStart(writer);

            // Add this component to the stack
            Push();

            // Get the content
            OnStart(writer);

            // Write any children
            WriteChildren(writer);
        }

        internal override void Finish(TextWriter writer)
        {
            // Only write content once
            if (_ended)
                return;
            _ended = true;

            // Stop now if not rendering
            if (!_render)
                return;

            // Provide finishing prior to popping from the stack
            PreFinish(writer);

            // Remove this component from the stack
            // This must be done before writing the end content in case there are pending components on the stack that need to be ended
            Pop(writer);

            // Get the content
            OnFinish(writer);
        }

        // This is implicit by definition since it's only ever used inside another component to generate content for a child, etc.
        internal override void StartAndFinish(TextWriter writer)
        {
            Start(writer, true);
            Finish(writer);
        }

        // This gets called before this component is pushed to the stack
        protected virtual void PreStart(TextWriter writer)
        {
        }

        protected abstract void OnStart(TextWriter writer);

        private void WriteChildren(TextWriter writer)
        {
            foreach (IComponent child in _children)
            {
                child.Start(writer, false);
                child.Finish(writer);
            }
        }

        // This gets called before this component is popped from the stack
        protected virtual void PreFinish(TextWriter writer)
        {
        }

        protected virtual void OnFinish(TextWriter writer)
        {
        }

        // Outputs the start and end portions together
        // This should only be used implicitly in a view and not from within this library (because of the way pending components are handled)
        // Instead, use Component.StartAndFinish() to write out the content of a component during Prepare, OnStart, or OnFinish
        public virtual string ToHtmlString()
        {
            // Remove this component from the pending list since we're outputting it directly
            // then output any remaining pending components so we have an accurate component stack
            PendingComponents.Remove(HtmlHelper, this);
            PendingComponents.Start(HtmlHelper);

            // Write this component out as a string
            using (StringWriter writer = new StringWriter())
            {
                Start(writer, false);
                Finish(writer);
                return writer.ToString();
            }
        }

        // The following code handles the stack of Bootstrap objects stored in the ViewContext

        private void Push()
        {
            GetStack().Push(this);
        }

        // This also writes end content from any components pending on the stack until this one
        private void Pop(TextWriter writer)
        {
            // Get the stack
            Stack<IComponent> stack = GetStack();

            // Peek components until we get to this one - the call to Finish() will pop them
            IComponent peek = null;
            while (stack.Count > 0 && (peek = stack.Peek()) != this && peek.Implicit)
            {
                peek.Finish(writer);
            }

            // Sanity check
            if (peek != this)
                throw new InvalidOperationException("A Bootstrap component is finishing but is not at the top of the stack, " +
                    "which is usually an indication that a component has been disposed out of order " +
                    "or that more than one component was created in a single using statement.");

            // Pop the component from the stack
            IComponent pop = stack.Pop();
            if (pop != this)
                throw new InvalidOperationException("Popped a different Bootstrap component from the stack (you should never see this).");
        }

        // This pops up the stack if (and only if) it is assignable to the specified type and it (and all intermediate components) are implicit
        // Use this to clear arbitrary implicitly added components from the stack (see how Tables.Row works)
        internal void Pop<TComponent>(TextWriter writer)
            where TComponent : IComponent
        {
            Stack<IComponent> stack = GetStack();

            // Crawl the stack and queue the components in case an intermediate is not implicit
            Queue<IComponent> finish = new Queue<IComponent>();
            if (stack.Count > 0)
            {
                foreach (IComponent component in stack)
                {
                    if (!component.Implicit)
                    {
                        break;
                    }
                    finish.Enqueue(component);
                    if (typeof(TComponent).IsAssignableFrom(component.GetType()))
                    {
                        // Found the type we were looking for, go ahead and finish it and the intermediates
                        while (finish.Count > 0)
                        {
                            finish.Dequeue().Finish(writer);
                        }
                        break;
                    }
                }
            }
        }

        // This pops up the stack if (and only if) the requested component and all intermediate components are implicit
        // Use this to clear specific implicitly added components from the stack (see how Forms.Input works)
        internal void Pop(IComponent pop, TextWriter writer)
        {
            if (pop == null || !pop.Implicit)
                return;
            Stack<IComponent> stack = GetStack();

            // Crawl the stack and queue the components in case an intermediate is not implicit
            Queue<IComponent> finish = new Queue<IComponent>();
            if (stack.Count > 0)
            {
                foreach (IComponent component in stack)
                {
                    if (!component.Implicit)
                    {
                        break;
                    }
                    finish.Enqueue(component);
                    if (component == pop)
                    {
                        // Found the component we were looking for, so go ahead and finish it and the intermediates
                        while (finish.Count > 0)
                        {
                            finish.Dequeue().Finish(writer);
                        }
                        break;
                    }
                }
            }
        }

        // Only the simple interface types should be used as generic type parameters here
        // Using a type that has TModel will skip components with a different model (such as when run from inside a partial with a different model)
        internal TComponent GetComponent<TComponent>()
            where TComponent : class, IComponent
        {
            return GetStack().Where(x => typeof(TComponent).IsAssignableFrom(x.GetType())).FirstOrDefault() as TComponent;
        }

        private Stack<IComponent> GetStack()
        {
            IDictionary items = ViewContext.HttpContext.Items;
            Stack<IComponent> stack = items[Bootstrap.ComponentStackKey] as Stack<IComponent>;
            if (stack == null)
            {
                stack = new Stack<IComponent>();
                items[Bootstrap.ComponentStackKey] = stack;
            }
            return stack;
        }
    }
}
