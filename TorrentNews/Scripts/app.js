//function configureDetailsButton() {
//    $('.detailsButton').click(function() {
//        var url = $(this).attr('data-details-url');
//        window.open(url, '_blank');
//    });
//}

function showErrorPopup(message) {
    var $popup = $('#errorPopup');

    $popup.find('div.popupMessage').text(message);
    $popup.find('button.closeButton').click(function () {
        $('#errorPopup').dialog('close');
    });

    $popup.dialog({
        dialogClass: "no-title",
        draggable: false,
        modal: true,
        resizable: false
    });
}

function showSignUpPopup() {
    var $popup = $('#signUpPopup');

    $popup.find('button.closeButton').click(function () {
        $('#signUpPopup').dialog('close');
    });

    $popup.dialog({
        dialogClass: "no-title",
        draggable: false,
        modal: true,
        resizable: false
    });
}

function configureTrailerButton() {
    $('.trailerButton').click(function () {
        var that = this;

        var $button = $(that);

        if ($button.hasClass('pushed')) {
            var $trailerRows = $('.trailerContainerRow');
            $trailerRows.remove();

            $button.removeClass('pushed');

            $button.parent().removeClass('animate-spinner');
        } else {
            $button.parent().addClass('animate-spinner');

            $('.trailerButton.pushed').removeClass('pushed');

            var $trailerRows = $('.trailerContainerRow');
            $trailerRows.remove();

            $button.addClass('pushed');

            $.ajax({
                url: $(that).attr('data-trailer-url'),
                context: that
            }).done(function (data) {
                var $btn = $(this);
                
                var $tr = $($btn.closest('tr'));
                $tr.after('<tr class="trailerContainerRow"><td colspan="4">' + data + '</td></tr>');

                $('.closeTrailer').click(function () {
                    $('.trailerButton.pushed').removeClass('pushed');

                    var $trailerRows = $('.trailerContainerRow');
                    $trailerRows.remove();

                    var torrentId = $(this).attr('data-torrent-id');
                    $.scrollTo($('tr[data-torrent-id=' + torrentId + ']'), 700);
                });

                $.scrollTo($('.trailerContainerRow'), 1700);
            }).always(function () {
                var $btn = $(this);
                $btn.parent().removeClass('animate-spinner');
            });
        }
    });
}

function configureTooltips() {
    $('.hasTooltip').hover(function () {
        var $that = $(this);
        $that.next('.tooltip').show(200);
    }, function () {
        var $that = $(this);
        $that.next('.tooltip').hide(0);
    });
}

function configureStarButtons() {
    $('.star').click(function () {
        var $this = $(this);
        var id = $this.attr('data-torrent-id');
        
        if ($this.hasClass('add-star')) {
            var url = '/Torrents/StarAdd?imdbId=' + id;
        } else {
            var url = '/Torrents/StarRemove?imdbId=' + id;
        }

        ajaxStarAddRemove(url, $this);
    });
}

function ajaxStarAddRemove(url, button) {
    
    if (button.parent().hasClass('animate-spinner')) {
        return;
    }

    button.parent().addClass('animate-spinner');
    
    $.ajax({
        type: "POST",
        url: url,
        context: button,
        dataType: "json",
        beforeSend: function (xhr, settings) {
            xhr.returnUrl = settings.url;
        }
    }).done(function (data) {
        
        var $btn = this;
        $btn.toggleClass('remove-star icon-star');
        $btn.toggleClass('add-star icon-star-empty');
        
    }).fail(function (xhr) {
        
        if (xhr.status == 403) {
            showSignUpPopup();

        } else {
            if (xhr.statusText) {
                showErrorPopup(xhr.statusText);
            } else {
                showErrorPopup('An unexpected error has occurred. Error code: ' + xhr.status);
            }
        }
    }).always(function () {
        var $btn = this;
        $btn.parent().removeClass('animate-spinner');
    });
}

function configureBookmarks() {
    $('#moviesTable td.bookmarkCell div.bookmark').click(function() {

        var button = $(this);

        if (button.hasClass('animate-spinner') || button.hasClass('bookmark-set')) {
            return;
        }

        button.addClass('animate-spinner');

        var date = button.attr('data-torrent-addedon');
        var id = button.attr('data-torrent-id');
        
        $.ajax({
            type: "POST",
            url: '/Torrents/SetBookmark?date=' + date + '&id=' + id,
            context: button,
            dataType: "json"
        }).done(function (data) {

            var bmSet = $('.bookmark-set');
            bmSet.toggleClass('bookmark-set bookmark-unset');
            bmSet.find('span.button-with-spinner').toggleClass('icon-bookmark icon-bookmark-empty');
            
            var btn = this;
            btn.toggleClass('bookmark-set bookmark-unset');
            btn.find('span.button-with-spinner').toggleClass('icon-bookmark icon-bookmark-empty');

        }).fail(function (xhr) {

            if (xhr.status == 403) {
                showSignUpPopup();

            } else {
                if (xhr.statusText) {
                    showErrorPopup(xhr.statusText);
                } else {
                    showErrorPopup('An unexpected error has occurred. Error code: ' + xhr.status);
                }
            }
        }).always(function () {
            var btn = this;
            btn.removeClass('animate-spinner');
        });
    });
}