using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace UzonMailDesktop.MVVM
{
    public class ObservableObject : INotifyPropertyChanged
    {
        protected virtual bool ThrowOnInvalidPropertyName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                propertyChanged(this, e);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            RaisePropertyChanged(propertyName);
        }

        protected virtual void NotifyOfPropertyChange<TProperty>(Expression<Func<TProperty>> property)
        {
            RaisePropertyChanged(GetMemberInfo(property).Name);
        }

        protected virtual void OnPropertyChanged<TProperty>(Expression<Func<TProperty>> property)
        {
            RaisePropertyChanged(GetMemberInfo(property).Name);
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName) && TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string message = "Invalid property name: " + propertyName;
                if (ThrowOnInvalidPropertyName)
                {
                    throw new ArgumentException(message);
                }
            }
        }

        private MemberInfo GetMemberInfo(Expression expression)
        {
            LambdaExpression lambdaExpression = (LambdaExpression)expression;
            MemberExpression memberExpression = !(lambdaExpression.Body is UnaryExpression expression1)
                ? (MemberExpression)lambdaExpression.Body
                : (MemberExpression)expression1.Operand;
            return memberExpression.Member;
        }
    }
}
