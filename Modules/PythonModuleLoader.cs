using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using log4net;
using Nini.Config;
using OpenMetaverse;

using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using IronPython.Runtime;

namespace PythonModuleLoader
{
    public class PythonRegionModuleHook : IRegionModule
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        Scene m_scene;
		IConfigSource m_config;
		
        private ScriptEngine m_pyengine = null;
        private ScriptScope m_pyscope = null;
		
        #region IRegionModule interface

        public void Initialise(Scene scene, IConfigSource config)
        {
            m_log.Info("[PythonModuleLoader] Initializing...");
            m_scene = scene;
			m_config = config;
            m_pyengine = Python.CreateEngine();
            m_pyscope = m_pyengine.CreateScope();
            ICollection<string> paths = m_pyengine.GetSearchPaths();
            paths.Add(AppDomain.CurrentDomain.BaseDirectory);
			paths.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptEngines"));
			m_pyengine.SetSearchPaths(paths);
			m_log.Info("Added " + AppDomain.CurrentDomain.BaseDirectory + " to python module search path");
        }

        public void PostInitialise()
        {
            ScriptSource source = null;
			m_pyscope.SetVariable("scene", m_scene);
			m_pyscope.SetVariable("config", m_config);
            source = m_pyengine.CreateScriptSourceFromString(
			                                				 "try:\n" +
			                                                 "  import pymodloader\n" +
			                                                 "  pymodloader.sceneinit(scene, config)\n" +
			                                                 "except Exception, e:\n" +
			                                                 "  import traceback\n" +
			                                                 "  traceback.print_exc()\n",
			                                                 SourceCodeKind.Statements);
            source.Execute(m_pyscope);
       }

		public void Close()
        {
        }

        public string Name
        {
            get { return "Python module loader"; }
        }

        public bool IsSharedModule
        {
            get { return false; }
        }

        #endregion

    }
}
