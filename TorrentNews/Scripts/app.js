//function configureDetailsButton() {
//    $('.detailsButton').click(function() {
//        var url = $(this).attr('data-details-url');
//        window.open(url, '_blank');
//    });
//}

function showErrorPopup(message) {
    var $popup = $('#errorPopup');

    $popup.find('span.errorMessage').text(message);
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

function configureTrailerButton() {
    $('.trailerButton').click(function () {
        var that = this;

        var $button = $(that);

        if ($button.hasClass('pushed')) {
            var $trailerRows = $('.trailerContainerRow');
            $trailerRows.remove();

            $button.removeClass('pushed');
        } else {
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

function configureWatchlistButtons() {
    $('.watchlist').click(function () {
        var $this = $(this);
        var id = $this.attr('data-torrent-id');
        
        if ($this.hasClass('add-watchlist')) {
            var url = '/Torrents/WatchlistAdd?imdbId=' + id;
        } else {
            var url = '/Torrents/WatchlistRemove?imdbId=' + id;
        }

        ajaxWatchlistAddRemove(url, $this);
    });
}

function ajaxWatchlistAddRemove(url, button) {
    
    if (button.hasClass('animation-watchlist')) {
        return;
    }
    
    button.addClass('animation-watchlist');
    
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
        $btn.toggleClass('remove-watchlist icon-star');
        $btn.toggleClass('add-watchlist icon-star-empty');
        $btn.removeClass('animation-watchlist');
        
    }).fail(function (xhr) {
        
        if (xhr.status == 403) {
            var $btn = this;
            $btn.removeClass('animation-watchlist');
            showErrorPopup('You are not logged in.');
        } else {
            if (xhr.statusText) {
                showErrorPopup(xhr.statusText);
            } else {
                showErrorPopup('An unexpected error has occurred. Error code: ' + xhr.status);
            }
        }
    });
}
