﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentBootstrap.Forms
{
    public class Label : Tag, FluentBootstrap.Grids.IGridColumn
    {
        public interface ICreate : ICreateComponent
        {
        }

        internal Label(BootstrapHelper helper, string label) : base(helper, "label", "control-label")
        {
            AddChild(new Content(helper, label));
        }

        protected override void Prepare(System.IO.TextWriter writer)
        {
            base.Prepare(writer);

            // Set default column classes if we're horizontal and we haven't already written one
            Form form = GetComponent<Form>();
            FormGroup formGroup = GetComponent<FormGroup>();
            if (form != null && form.Horizontal && form.DefaultLabelWidth != null 
                && formGroup != null && !formGroup.WroteLabel
                && !CssClasses.Any(x => x.StartsWith("col-")))
            {
                this.Md(form.DefaultLabelWidth);
            }
        }

        protected override void OnStart(System.IO.TextWriter writer)
        {
            base.OnStart(writer);

            // Track that a label was written as part of this form group
            FormGroup formGroup = GetComponent<FormGroup>();
            if (formGroup != null)
            {
                formGroup.WroteLabel = true;
            }
        }
    }
}