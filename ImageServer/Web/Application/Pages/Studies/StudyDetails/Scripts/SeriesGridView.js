/* License
 *
 * Copyright (c) 2011, ClearCanvas Inc.
 * All rights reserved.
 * http://www.clearcanvas.ca
 *
 * This software is licensed under the Open Software License v3.0.
 * For the complete license, see http://www.clearcanvas.ca/OSLv3.0
 *
 */

/////////////////////////////////////////////////////////////////////////////////////////////////////////
///
/// This script contains the javascript component class for the study search panel
/// 
/////////////////////////////////////////////////////////////////////////////////////////////////////////

// Define and register the control type.
//
// Only define and register the type if it doens't exist. Otherwise "... does not derive from Sys.Component" error 
// will show up if multiple instance of the controls must be created. The error is misleading. It looks like the type 
// is RE-define for the 2nd instance but registerClass() will fail so the type will be essential undefined when the object
// is instantiated.
//
if (window.__registeredTypes['ClearCanvas.ImageServer.Web.Application.Pages.Studies.StudyDetails.Controls.SeriesGridView']==null)
{
    Type.registerNamespace('ClearCanvas.ImageServer.Web.Application.Pages.Studies.StudyDetails.Controls');

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    // Constructor
    //
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    ClearCanvas.ImageServer.Web.Application.Pages.Studies.StudyDetails.Controls.SeriesGridView = function(element) { 
        ClearCanvas.ImageServer.Web.Application.Pages.Studies.StudyDetails.Controls.SeriesGridView.initializeBase(this, [element]);
       
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    // Create the prototype for the control.
    //
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    ClearCanvas.ImageServer.Web.Application.Pages.Studies.StudyDetails.Controls.SeriesGridView.prototype = 
    {
        initialize : function() {
            ClearCanvas.ImageServer.Web.Application.Pages.Studies.StudyDetails.Controls.SeriesGridView.callBaseMethod(this, 'initialize');        
            
            this._OnLoadHandler = Function.createDelegate(this,this._OnLoad);
            this._OnSeriesListDoubleClickedHandler = Function.createDelegate(this,this._OnSeriesListDoubleClicked);
            
            Sys.Application.add_load(this._OnLoadHandler);
                 
        },
        
        dispose : function() {
            $clearHandlers(this.get_element());

            ClearCanvas.ImageServer.Web.Application.Pages.Studies.StudyDetails.Controls.SeriesGridView.callBaseMethod(this, 'dispose');
            
            Sys.Application.remove_load(this._OnLoadHandler);
        },
        
        
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Events
        //
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        
        /// called whenever the page is reloaded or partially reloaded
        _OnLoad : function()
        {
            var serieslist = $find(this._SeriesListClientID);
            if (serieslist!=null)
            {
                serieslist.add_onClientRowDblClick(this._OnSeriesListDoubleClickedHandler);
            }
        },
        
        // called when the user double click on the series list
        _OnSeriesListDoubleClicked : function(src, event)
        {
            var serieslist = $find(this._SeriesListClientID);
            if (serieslist!=null)
            {
                var rows = serieslist.getSelectedRowElements();
                for(i=0; i<rows.length; i++)
                {
                    var url = String.format('{0}?serverae={1}&studyuid={2}&seriesuid={3}', 
                                this._OpenSeriesPageUrl, 
                                this._getServerAE(rows[i]), 
                                this._getStudyUid(rows[i]), 
                                this._getSeriesUid(rows[i]));
                    window.open(url);
                }    
            }
        },
        
        
        
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Private Methods
        //
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        _getServerAE : function (row)
        {
            return row.getAttribute('serverae');
        },
        
        _getStudyUid : function (row)
        {
            return row.getAttribute('studyuid');
        },
        
        _getSeriesUid: function (row)
        {
            return row.getAttribute('seriesuid');
        },

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Public methods
        //
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Properties
        //
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        get_SeriesListClientID : function() {
            return this._SeriesListClientID;
        },

        set_SeriesListClientID: function(value) {
            this._SeriesListClientID = value;
            this.raisePropertyChanged('SeriesListClientID');
        },
        
        
        
        get_OpenSeriesPageUrl : function() {
            return this._OpenSeriesPageUrl;
        },
       
        set_OpenSeriesPageUrl : function(value) {
            this._OpenSeriesPageUrl = value;
            this.raisePropertyChanged('OpenSeriesPageUrl');
        }
        

    }

    // Register the class as a type that inherits from Sys.UI.Control.

        ClearCanvas.ImageServer.Web.Application.Pages.Studies.StudyDetails.Controls.SeriesGridView.registerClass('ClearCanvas.ImageServer.Web.Application.Pages.Studies.StudyDetails.Controls.SeriesGridView', Sys.UI.Control);
     

    if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();

}