/// <reference path="../../../vendor/jquery/jquery.js" />
/// <reference path="../../../vendor/jstree/jstree.js" />
/*
Name: 			UI Elements / Tree View - Examples
Written by: 	Okler Themes - (http://www.okler.net)
Theme Version: 	1.3.0
*/

(function ($) {
    'use strict';

    /*
	Basic
	*/
    function getSelectedItems() {
        var ids = $("#treeCheckbox").jstree("get_selected");
        //project="@project" application="@jar.Name" cluster="@cluster.Key" access="write"
        var selected = [];
        // alert(ids.length);
        for (var i = 0; i < ids.length; i++) {
            var node = ids[i];
            var parent = $($("#treeCheckbox").jstree("get_json", node).text);
            if (parent) {
                var input = parent.find("input");

                if (input.attr("project")) {

                    var item = {
                        project: input.attr("project"),
                        application: input.attr("application"),
                        cluster: input.attr("cluster"),
                        access: input.attr("access")
                    }
                    selected.push(item);
                }
            }
        }
        // alert(selected.length);
        return selected;
    }

    $(document).ready(function () {
        $("#save-access-rules").click(function () {
            $.ajax({
                type: "POST",
                url: window.location,
                data: { access: getSelectedItems() }
            });
        });

        $('#treeCheckbox').jstree('close_all');
    });

}).apply(this, [jQuery]);