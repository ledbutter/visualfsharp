// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections;
using System.Text;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using IServiceProvider = System.IServiceProvider;
using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;

namespace Microsoft.VisualStudio.FSharp.ProjectSystem
{

    internal abstract class SelectionListener : IVsSelectionEvents, IDisposable
    {
        #region fields
        private uint eventsCookie = (uint)ShellConstants.VSCOOKIE_NIL;
        private IVsMonitorSelection monSel = null;
        private ServiceProvider serviceProvider = null;
        private bool isDisposed;
        /// <summary>
        /// Defines an object that will be a mutex for this object for synchronizing thread calls.
        /// </summary>
        private static volatile object Mutex = new object();
        #endregion

        #region ctors
        public /*protected, but public for FSharp.Project.dll*/ SelectionListener(ServiceProvider serviceProvider)
        {

            this.serviceProvider = serviceProvider;
            this.monSel = serviceProvider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;

            Debug.Assert(this.monSel != null, "Could not get the IVsMonitorSelection object from the services exposed by this project");

            if (this.monSel == null)
            {
                throw new InvalidOperationException();
            }
        }
        #endregion

        #region properties
        public /*protected, but public for FSharp.Project.dll*/ uint EventsCookie
        {
            get
            {
                return this.eventsCookie;
            }
        }

        public /*protected, but public for FSharp.Project.dll*/ IVsMonitorSelection SelectionMonitor
        {
            get
            {
                return this.monSel;
            }
        }

        public /*protected, but public for FSharp.Project.dll*/ ServiceProvider ServiceProvider
        {
            get
            {
                return this.serviceProvider;
            }
        }
        #endregion

        #region IVsSelectionEvents Members

        public virtual int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IDisposable Members
        /// <summary>
        /// The IDispose interface Dispose method for disposing the object determinastically.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region methods
        public void Init()
        {
            if (this.SelectionMonitor != null)
            {
                this.SelectionMonitor.AdviseSelectionEvents(this, out this.eventsCookie);
            }
        }

        /// <summary>
        /// The method that does the cleanup.
        /// </summary>
        /// <param name="disposing"></param>
        public /*protected, but public for FSharp.Project.dll*/ virtual void Dispose(bool disposing)
        {
            // Everybody can go here.
            if (!this.isDisposed)
            {
                // Synchronize calls to the Dispose simulteniously.
                lock (Mutex)
                {
                    if (!this.isDisposed)
                    {
                        if (disposing && this.eventsCookie != (uint)ShellConstants.VSCOOKIE_NIL && this.SelectionMonitor != null)
                        {
                            this.SelectionMonitor.UnadviseSelectionEvents((uint)this.eventsCookie);
                            this.eventsCookie = (uint)ShellConstants.VSCOOKIE_NIL;
                        }

                        this.isDisposed = true;
                    }
                }
            }
        }
        #endregion

    }
}
