export function initGallery() {
  document.querySelectorAll('#media-gallery .blueimp-img').forEach(img => {
    img.addEventListener('click', (event) => {
      var target = event.target;
      var link = target.src ? target.parentNode : target
      var options = {
        index: link,
        event: event,
        onclosed: function () {
          //ensure body scrollbar is shown if hidden
          document.body.style.overflow = "";
        }
      }
      var links = document.querySelectorAll('#media-gallery .blueimp-img');
      blueimp.Gallery(links, options);
    });
  });
}