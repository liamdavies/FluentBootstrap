﻿using FluentBootstrap.Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentBootstrap.Forms
{
    internal interface IFormGroup : ITag
    {
        ILabel Label { set; }
    }

    public interface IFormGroupCreator<TModel> : IComponentCreator<TModel>
    {
    }

    public class FormGroup<TModel> : Tag<TModel, FormGroup<TModel>>, IFormGroup, FluentBootstrap.Grids.IHasGridColumnExtensions, IFormValidation,
        ILabelCreator<TModel>,
        IFormControlCreator<TModel>,
        IHelpBlockCreator<TModel>
    {
        private ILabel _label = null;
        private Element<TModel> _columnWrapper;
        private bool _columnWrapperBeforeLabel = false;

        internal ILabel Label
        {
            set
            {
                _label = value;                
                PendingComponents.Remove(HtmlHelper, value);    // Need to remove this from the pending components since it's similar to a child and will be output from this form control
            }
        }

        ILabel IFormGroup.Label
        {
            set { Label = value; }
        }

        internal bool HasLabel
        {
            get { return _label != null; }
        }

        internal bool? Horizontal { get; set; }

        internal FormGroup(BootstrapHelper<TModel> helper)
            : base(helper, "div", Css.FormGroup)
        {

        }

        protected override void PreStart(TextWriter writer)
        {
            base.PreStart(writer);

            // Set column classes if we're horizontal          
            IForm form = GetComponent<IForm>();
            if ((form != null && form.Horizontal && (!Horizontal.HasValue || Horizontal.Value)) || (Horizontal.HasValue && Horizontal.Value))
            {
                int labelWidth = form == null ? Bootstrap.DefaultFormLabelWidth : form.DefaultLabelWidth;

                // Set label column class
                if (_label != null && !_label.CssClasses.Any(x => x.StartsWith("col-")))
                {
                    _label.SetColumnClass("col-md-", labelWidth);
                }

                // Add column classes to this (these will get moved to a wrapper later in this method)
                if (!CssClasses.Any(x => x.StartsWith("col-")))
                {
                    this.Md(Bootstrap.GridColumns - labelWidth);

                    // Also need to add an offset if no label
                    if (_label == null)
                    {
                        this.MdOffset(labelWidth);
                    }
                }
            }
            else if (form != null && form.Horizontal)
            {
                // If the form is horizontal but we requested not to be, create a full-width column wrapper
                this.Md(Bootstrap.GridColumns);
                _columnWrapperBeforeLabel = true;
            }

            // Move any grid column classes to a container class
            if (CssClasses.Any(x => x.StartsWith("col-")))
            {
                _columnWrapper = new Element<TModel>(Helper, "div", CssClasses.Where(x => x.StartsWith("col-")).ToArray());
                PendingComponents.Remove(HtmlHelper, _columnWrapper);    // Need to remove this from the pending components since it'll be output during OnStart
            }
            CssClasses.RemoveWhere(x => x.StartsWith("col-"));
        }

        protected override void OnStart(TextWriter writer)
        {
            base.OnStart(writer);

            // Write the column wrapper first if needed
            if (_columnWrapperBeforeLabel && _columnWrapper != null)
            {
                _columnWrapper.Start(writer, true);
            }

            // Write the label
            if (_label != null)
            {
                _label.StartAndFinish(writer);
            }

            // Write the column wrapper
            if (!_columnWrapperBeforeLabel && _columnWrapper != null)
            {
                _columnWrapper.Start(writer, true);
            }
        }

        protected override void OnFinish(TextWriter writer)
        {
            Pop(_columnWrapper, writer);
            base.OnFinish(writer);
        }
    }
}
