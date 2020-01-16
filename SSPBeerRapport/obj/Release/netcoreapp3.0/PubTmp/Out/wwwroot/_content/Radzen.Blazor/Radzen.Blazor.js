if (!Element.prototype.matches) {
    Element.prototype.matches = Element.prototype.msMatchesSelector ||
        Element.prototype.webkitMatchesSelector;
}

if (!Element.prototype.closest) {
    Element.prototype.closest = function (s) {
        var el = this;

        do {
            if (el.matches(s)) return el;
            el = el.parentElement || el.parentNode;
        } while (el !== null && el.nodeType === 1);
        return null;
    };
}

var Radzen = {
    uploads: function (uploadComponent, id) {
        if (!Radzen.uploadComponents) {
            Radzen.uploadComponents = {};
        }
        Radzen.uploadComponents[id] = uploadComponent;
    },
    upload: function (fileInput, url, multiple) {
        var data = new FormData();
        for (var i = 0; i < fileInput.files.length; i++) {
            var file = fileInput.files[i];
            data.append(multiple ? 'files' : 'file', file, file.name);
        }
        var xhr = new XMLHttpRequest();
        xhr.upload.onprogress = function (e) {
            if (e.lengthComputable) {
                var uploadComponent = Radzen.uploadComponents && Radzen.uploadComponents[fileInput.id];
                if (uploadComponent) {
                    var progress = parseInt((e.loaded / e.total) * 100);
                    uploadComponent.invokeMethodAsync("RadzenUpload.OnProgress", progress, e.loaded, e.total);
                }
            }
        };
        xhr.open('POST', url, true);
        xhr.send(data);
    },
    getCookie : function (name) {
        var value = "; " + decodeURIComponent(document.cookie);
        var parts = value.split("; " + name + "=");
        if (parts.length == 2) return parts.pop().split(";").shift();
    },
    getCulture: function() {
        var cultureCookie = Radzen.getCookie(".AspNetCore.Culture");
        var uiCulture = cultureCookie ? cultureCookie.split('|').pop().split('=').pop() : null;
        return uiCulture || 'en-US';
    },
    numericOnPaste: function (e) {
        if (e.clipboardData) {
          var value = e.clipboardData.getData('text');

          if (value && /^-?\d*\.?\d*$/.test(value)) {
            return;
          }

          e.preventDefault();
        }
    },
    numericKeyPress: function (e) {
        if (e.metaKey || e.ctrlKey || e.keyCode == 9 || e.keyCode == 8 || e.keyCode == 13) {
            return;
        }

        var ch = String.fromCharCode(e.charCode);

        if (/^[-\d,.]$/.test(ch)) {
            return;
        }

        e.preventDefault();
    },
    openPopup: function (e, el, popup, isDropDown, additionalStyle) {
        var handler = function () {
            popup.isOpen = false;
            popup.style.display = 'none';
            if (popup.parentNode) {
                popup.parentNode.classList.remove('ui-dropdown-open');
            }

            window.removeEventListener('wheel', handler);
            window.removeEventListener('scroll', handler);

            var d = popup.closest('.ui-dialog-content');
            if (d) {
                d.removeEventListener('wheel', handler);
                d.removeEventListener('scroll', handler);
            }
        };
        window.addEventListener('wheel', handler);
        window.addEventListener('scroll', handler);

        var dialog = popup.closest('.ui-dialog-content');
        if (dialog) {
            dialog.addEventListener('wheel', handler);
            dialog.addEventListener('scroll', handler);
        }

        var style = isDropDown ? 'z-index: 1005; transform: translateY(0px); opacity: 1; display:block; position: fixed;' + Radzen.getPopupWidthAndTop(popup, isDropDown) + ';' + additionalStyle
            : 'margin-left: 0px; z-index: 1002; transform: translateY(0px); opacity: 1; position: fixed;' + Radzen.getPopupWidthAndTop(popup, isDropDown) + ';' + additionalStyle;

        popup.isOpen = !popup.isOpen;

        popup.style.cssText = popup.isOpen ? style : 'display:none;';

        if (popup.parentNode) {
            if (popup.style.display == 'none') {
                popup.parentNode.classList.remove('ui-dropdown-open');
            } else {
                popup.parentNode.classList.add('ui-dropdown-open');
            }
        }
    },
    closePopup: function (e, el, popup) {
        if (!el.contains(e.relatedTarget) && popup) {
            Radzen.hidePopup(popup);
        }
    },
    hidePopup: function (popup) {
        popup.style.display = 'none';
        popup.isOpen = false;
        if (popup.parentNode) {
            popup.parentNode.classList.remove('ui-dropdown-open');
        }
    },
    scrollDataGrid: function (e) {
        document.body.querySelectorAll('.ui-autocomplete-panel, .ui-overlaypanel, .ui-dropdown-panel, .ui-datepicker:not(.ui-datepicker-inline)').forEach(function (p) { Radzen.hidePopup(p); });
        var scrollLeft = (e.target.scrollLeft ? '-' + e.target.scrollLeft : 0) + 'px';

        e.target.previousElementSibling.style.marginLeft = scrollLeft;

        if (e.target.nextElementSibling) {
            e.target.nextElementSibling.style.marginLeft = scrollLeft;
        }
    },
    openDialog: function () {
        document.body.classList.add('no-scroll');
    },
    closeDialog: function () {
        document.body.classList.remove('no-scroll');
    },
    getInputValue: function (arg) {
        var input = (arg instanceof Element || arg instanceof HTMLDocument) ? arg : document.getElementById(arg);
        return input ? input.value : '';
    },
    setInputValue: function (arg,value) {
        var input = (arg instanceof Element || arg instanceof HTMLDocument) ? arg : document.getElementById(arg);
        if (input) {
            input.value = value;
        }
    },
    getPopupWidthAndTop: function (popup, isDropDown) {
        var el = popup.parentNode;
        var rect = el.getBoundingClientRect();
        var top = rect.top + rect.height;

        popup.style.cssText = 'visibility:hidden;'

        var popupHeight = popup.offsetHeight;
        var popupWidth = popup.classList.contains('ui-datepicker') || popup.classList.contains('ui-timepicker') ? 320 : popup.offsetWidth;

        if (rect.bottom + popupHeight >= window.innerHeight) {
            top = top - rect.height - popupHeight;
        }

        popup.style.cssText = 'display:none;'

        var style = popup.classList.contains('ui-autocomplete-panel') ? 'width:' + rect.width + 'px; top:' + top + 'px;':
                'width:calc(' + rect.width + 'px - 1.425rem); top:' + top + 'px;';

        if (rect.left + popupWidth >= window.innerWidth) {
            style = style + 'right:0px;';
        }

        if (!popup.classList.contains('ui-overlaypanel')) {
            style = style + 'left:' + rect.left + 'px;';
        }

        return style;
    },
    readFileAsBase64: function (fileInput) {
        var readAsDataURL = function (fileInput) {
            return new Promise(function (resolve, reject) {
                var reader = new FileReader();
                reader.onerror = function () {
                    reader.abort();
                    reject(new Error("Error reading file."));
                };
                reader.addEventListener("load", function () {
                    resolve(reader.result);
                }, false);
                reader.readAsDataURL(fileInput.files[0]);
            });
        };

        return readAsDataURL(fileInput);
    },
    closeMenuItems: function (event) {
        var menu = event.target.closest('.menu');

        if (!menu) {
            var targets = document.querySelectorAll('.navigation-item-wrapper-active');

            if (targets) {
                for (var i = 0; i < targets.length; i++) {
                    Radzen.toggleMenuItem(targets[i], false);
                }
            }
            document.removeEventListener('click', Radzen.closeMenuItems);
        }
    },
    closeOtherMenuItems: function (current) {
        var targets = document.querySelectorAll('.navigation-item-wrapper-active');
        if (targets) {
            for (var i = 0; i < targets.length; i++) {
                var target = targets[i];
                var item = target.closest('.navigation-item');

                if (!current || !item.contains(current)) {
                    Radzen.toggleMenuItem(target, false);
                }
            }
        }
    },
    toggleMenuItem: function (target, selected) {
        Radzen.closeOtherMenuItems(target);

        var item = target.closest(".navigation-item");

        if (selected === undefined) {
            selected = !item.classList.contains('navigation-item-active');
        }

        item.classList.toggle('navigation-item-active', selected);

        target.classList.toggle('navigation-item-wrapper-active', selected)

        var children = item.querySelector('.navigation-menu');

        if (children) {
            children.style.display = selected ? '' : 'none';
        } else if (selected) {
            Radzen.closeOtherMenuItems(null);
        }

        var icon = item.querySelector('.navigation-item-icon-children');

        if (icon) {
            var deg = selected ? '180deg' : 0;
            icon.style.transform = 'rotate(' + deg + ')';
        }

        document.removeEventListener('click', Radzen.closeMenuItems);
        document.addEventListener('click', Radzen.closeMenuItems);
    }
};