﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using FluentBootstrap.Forms;

namespace FluentBootstrap
{
    public static class FormExtensions
    {
        // Form

        public static Form Form<TCreator>(this IComponentCreator<TCreator> creator, FormMethod method = FormMethod.Post)
            where TCreator : Form.ICreate
        {
            return new Form(creator.GetHelper()).Action(null).Method(method);
        }

        public static Form Form<TCreator>(this IComponentCreator<TCreator> creator, string action, FormMethod method = FormMethod.Post)
            where TCreator : Form.ICreate
        {
            return new Form(creator.GetHelper()).Action(action).Method(method);
        }

        public static Form Form<TCreator>(this IComponentCreator<TCreator> creator, string actionName, string controllerName, FormMethod method = FormMethod.Post)
            where TCreator : Form.ICreate
        {
            return new Form(creator.GetHelper()).Action(actionName, controllerName).Method(method);
        }

        public static Form Inline(this Form form, bool inline = true)
        {
            form.ToggleCssClass("form-inline", inline, "form-horizontal");
            return form;
        }

        public static Form Horizontal(this Form form, int? defaultLabelWidth = 2, bool horizontal = true)
        {
            form.ToggleCssClass("form-horizontal", horizontal, "form-inline");
            form.DefaultLabelWidth = defaultLabelWidth;
            return form;
        }

        // Use action = null to reset form action to current request url
        public static Form Action(this Form form, string action)
        {
            form.MergeAttribute("action", action == null ? form.ViewContext.HttpContext.Request.RawUrl : action);
            return form;
        }

        public static Form Action(this Form form, string actionName, string controllerName, object routeValues = null)
        {
            RouteValueDictionary routeValueDictionary = routeValues == null ? new RouteValueDictionary() : routeValues as RouteValueDictionary;
            if(routeValueDictionary == null)
                new RouteValueDictionary(routeValues);

            form.MergeAttribute("action", UrlHelper.GenerateUrl(null, actionName, controllerName, routeValueDictionary, 
                form.HtmlHelper.RouteCollection, form.ViewContext.RequestContext, true));
            return form;
        }

        public static Form Route(this Form form, string routeName, object routeValues = null)
        {
            RouteValueDictionary routeValueDictionary = routeValues == null ? new RouteValueDictionary() : routeValues as RouteValueDictionary;
            if (routeValueDictionary == null)
                new RouteValueDictionary(routeValues);

            form.MergeAttribute("action", UrlHelper.GenerateUrl(routeName, null, null, routeValueDictionary,
                form.HtmlHelper.RouteCollection, form.ViewContext.RequestContext, false));
            return form;
        }

        public static Form Method(this Form form, FormMethod method)
        {
            form.MergeAttribute("method", HtmlHelper.GetFormMethodString(method));
            return form;
        }

        // Form Group

        public static FormGroup FormGroup<TCreator>(this IComponentCreator<TCreator> creator)
            where TCreator : FormGroup.ICreate
        {
            return new FormGroup(creator.GetHelper());
        }

        // Label
        
        public static Label Label<TCreator>(this IComponentCreator<TCreator> creator, string label)
            where TCreator : Label.ICreate
        {
            return new Label(creator.GetHelper(), label);
        }

        public static Label For(this Label label, string @for)
        {
            label.MergeAttribute("for", @for);
            return label;
        }

        public static Label ScreenReaderOnly(this Label label, bool screenReaderOnly = true)
        {
            label.ToggleCssClass("sr-only", screenReaderOnly);
            return label;
        }

        // Input

        public static Input Input<TCreator>(this IComponentCreator<TCreator> creator, string name = null, string label = null, object value = null, string format = null, FormInputType inputType = FormInputType.Text)
            where TCreator : Input.ICreate
        {
            return new Input(creator.GetHelper(), inputType.GetDescription()).Name(name).ControlLabel(label).Value(value, format);
        }

        public static Input Value(this Input input, object value, string format = null)
        {
            input.MergeAttribute("value", value == null ? null : input.HtmlHelper.FormatValue(value, format));
            return input;
        }

        public static Input Placeholder(this Input input, string placeholder)
        {
            input.MergeAttribute("placeholder", placeholder);
            return input;
        }

        // TextArea

        public static TextArea TextArea<TCreator>(this IComponentCreator<TCreator> creator, string name = null, string label = null, object value = null, string format = null, int? rows = null)
            where TCreator : TextArea.ICreate
        {
            return new TextArea(creator.GetHelper()).Name(name).ControlLabel(label).Value(value, format).Rows(rows);
        }

        public static TextArea Rows(this TextArea textArea, int? rows)
        {
            textArea.MergeAttribute("rows", rows == null ? null : rows.Value.ToString());
            return textArea;
        }

        public static TextArea Value(this TextArea textArea, object value, string format = null)
        {
            textArea.Value = value == null ? null : textArea.HtmlHelper.FormatValue(value, format);
            return textArea;
        }

        public static TextArea Placeholder(this TextArea textArea, string placeholder)
        {
            textArea.MergeAttribute("placeholder", placeholder);
            return textArea;
        }

        // CheckedControl

        public static CheckedControl CheckBox<TCreator>(this IComponentCreator<TCreator> creator, string name = null, string label = null, string description = null, bool isChecked = false)
            where TCreator : CheckedControl.ICreate
        {
            return new CheckedControl(creator.GetHelper(), "checkbox").Name(name).ControlLabel(label).Description(description).IsChecked(isChecked);
        }

        public static CheckedControl Radio<TCreator>(this IComponentCreator<TCreator> creator, string name = null, string label = null, string description = null, object value = null, bool isChecked = false)
            where TCreator : CheckedControl.ICreate
        {
            return new CheckedControl(creator.GetHelper(), "radio").Name(name).ControlLabel(label).Description(description).Value(value).IsChecked(isChecked);
        }

        public static CheckedControl Description(this CheckedControl checkedControl, string description)
        {
            checkedControl.Description = description;
            return checkedControl;
        }

        public static CheckedControl Inline(this CheckedControl checkedControl, bool inline = true)
        {
            checkedControl.Inline = inline;
            return checkedControl;
        }

        public static CheckedControl IsChecked(this CheckedControl checkedControl, bool isChecked = true)
        {
            checkedControl.MergeAttribute("checked", isChecked ? "checked" : null);
            return checkedControl;
        }

        public static CheckedControl Value(this CheckedControl checkedControl, object value)
        {
            checkedControl.MergeAttribute("value", value == null ? null : checkedControl.HtmlHelper.FormatValue(value, null));
            return checkedControl;
        }

        // Select
        
        public static Select Select<TCreator>(this IComponentCreator<TCreator> creator, string name = null, string label = null, params object[] options)
            where TCreator : Select.ICreate
        {
            return new Select(creator.GetHelper()).Name(name).ControlLabel(label).Options(options);
        }

        public static Select Multiple(this Select select, bool multiple = true)
        {
            select.MergeAttribute("multiple", multiple ? "multiple" : null);
            return select;
        }

        public static Select Options(this Select select, params object[] options)
        {
            select.Options.Clear();
            foreach (object option in options)
            {
                select.Option(option);
            }
            return select;
        }

        public static Select Option(this Select select, object option, string format = null)
        {
            if (option != null)
            {
                select.Options.Add(select.HtmlHelper.FormatValue(option, format));
            }
            return select;
        }

        // FormControl
        
        public static TFormControl Name<TFormControl>(this TFormControl control, string name)
            where TFormControl : FormControl
        {
            control.MergeAttribute("name", name == null ? null : control.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name));
            return control;
        }

        public static TFormControl ControlLabel<TFormControl>(this TFormControl control, string label, Action<Label> labelAction = null)
            where TFormControl : FormControl
        {
            if(label != null)
            {
                control.Label = new Label(control.Helper, label);
                if (labelAction != null)
                {
                    labelAction(control.Label);
                }
            }
            return control;
        }
    }
}