var charts = new Object();

window.setupChart = (id, config) => {
    console.log(config);
    var ctx = document.getElementById(id);
    if (typeof charts[id] !== 'undefined') { charts[id].destroy(); }
    charts[id] = new Chart(ctx, config);
}