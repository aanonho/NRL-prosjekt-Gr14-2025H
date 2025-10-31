//obstacleForm.js

// Show/hide relevant input fields based on selected obstacle type
const obstacleTypeSelect = document.getElementById('ObstacleType');
const obstacleTypeHidden = document.getElementById('ObstacleTypeHidden')
const areaRadiusContainer = document.getElementById('areaRadiusContainer');
const lineLengthContainer = document.getElementById('lineLengthContainer');
const lineCoordinatesContainer = document.getElementById('lineCoordinatesContainer');

// Handle image upload: preview selected image and allow removing it
const fileInput = document.getElementById('ObstacleImage');
const imagePreview = document.getElementById('imagePreview');
const imageContainer = document.getElementById('imageContainer');
const removeButton = document.getElementById('removeImageButton');

    // Preview when user uploads an image
fileInput.addEventListener('change', function(event) {
    const file = event.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = function() {
        imagePreview.src = reader.result;
        imageContainer.classList.remove('hidden');
    };
    reader.readAsDataURL(file);
});

    // Remove image preview and reset input When user clicks remove button
removeButton.addEventListener('click', function() {
    fileInput.value = ""; // removes file input
    imagePreview.src = "";
    imageContainer.classList.add('hidden');
});

// Set submit type (Submit or SaveDraft)
window.setSubmitType = function (value) {
    document.getElementById('submitType').value = value;
}

// Leaflet map logic
document.addEventListener('DOMContentLoaded', function () {
    var map = L.map('map').setView([58.1467, 7.9956], 12);

    //OpenStreetMap title layer
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);

    var helicopterMarker;
    var obstacleMarker;
    var circle;
    var line;
    var latlngsLine = [];
    var lineMarkers = [];


    // Triggered when the obstacle type changes - stores the selected type
    obstacleTypeSelect.addEventListener('change', function () {
        const type = obstacleTypeSelect.value;

        // Show/hide radius input for area type
        areaRadiusContainer.style.display = (type === 'area') ? 'block' : 'none';

        // Show/hide height and length inputs for line/cable types
        const showLineInputs = (type === 'line' || type === 'cable');
        lineLengthContainer.style.display = showLineInputs ? 'block' : 'none';
        lineCoordinatesContainer.style.display = showLineInputs ? 'block' : 'none';

        // Clear map is type changes
        clearMapObstacle();
    });

    // Delete obstacle button logic
    const deleteButton = document.getElementById('deleteObstacleButton'); 
    deleteButton.style.display = 'none';
    function showDeleteButton() {
        deleteButton.style.display = 'inline-block';
    }

    // Reset line coordinates to start a new drawing after deletion
    function resetLineState() {
        latlngsLine = [];
        if (line) {
            map.removeLayer(line);
            line = null;
        }
        lineMarkers.forEach(m => map.removeLayer(m));
        lineMarkers = [];
    }

    function clearMapObstacle() {
        // Remove all drawn layers from map
        if (obstacleMarker) {map.removeLayer(obstacleMarker); obstacleMarker = null;}
        if (circle) { map.removeLayer(circle); circle = null; };

        resetLineState();
      
        // Clear all related hidden input fields
        document.getElementById('ObstacleGeoJson').value = '';
        document.getElementById('ObstacleLatitude').value = '';
        document.getElementById('ObstacleLongitude').value = '';
        document.getElementById('ObstacleLineLength').value = '';
        document.getElementById('ObstacleLineCoordinates').value = '';

        deleteButton.style.display = 'none';
    }

    deleteButton.addEventListener('click', clearMapObstacle);
  
    // Mast icon
    var mastIcon = L.icon({
        iconUrl: '/icons/mast.svg',
        iconSize: [32, 64],
        iconAnchor: [16, 64]
    });

    // Helicopter icon
    var helicopterIcon = L.icon({
        iconUrl: '/icons/helicopter.svg',
        iconSize: [60, 60],
        iconAnchor: [30, 30]
    });      

    // Handle map clicks to draw obstacls
    map.on('click', function (e) {
        const type = document.getElementById('ObstacleType').value;
  
        document.getElementById('ObstacleLatitude').value = '';
        document.getElementById('ObstacleLongitude').value = '';

        let addedObstacle = false; // True if an obstacle exists and can be deleted

        // Point or Mast
        if (type === 'point' || type === 'mast') // For point and mast, update fields directly
        {
            clearMapObstacle(); // Reset map and input fields 

            document.getElementById('ObstacleLatitude').value = e.latlng.lat.toFixed(6);
            document.getElementById('ObstacleLongitude').value = e.latlng.lng.toFixed(6);
            document.getElementById('ObstacleGeoJson').value = JSON.stringify({
                type: "Feature",
                geometry: {
                    type: "Point",
                    coordinates: [e.latlng.lng, e.latlng.lat]
                },
                properties: {}
            });

            if (type === 'point') {
                obstacleMarker = L.marker(e.latlng).addTo(map); // Add simple marker                 
            } else {
                obstacleMarker = L.marker(e.latlng, { icon: mastIcon }).addTo(map);  // Add mast icon marker                      
            }

            addedObstacle = true;
        }
        // Area
        else if (type === 'area') {
            clearMapObstacle(); // Reset map and input fields 

            const radiusInput = document.getElementById('ObstacleRadius');          
            const radius = parseFloat(radiusInput.value) || 300; // Default radius

            circle = L.circle(e.latlng, { color: 'red', fillColor: '#f03', fillOpacity: 0.5, radius: radius }).addTo(map); // Add circle with radius          

            // Update hidden fields
            document.getElementById('ObstacleRadius').value = radius; 
            document.getElementById('ObstacleLatitude').value = e.latlng.lat.toFixed(6);
            document.getElementById('ObstacleLongitude').value = e.latlng.lng.toFixed(6);
            // Create GeoJSON for circle center point with radius property
            document.getElementById('ObstacleGeoJson').value = JSON.stringify(
                {
                    "type": "Feature",
                    geometry: {
                        "type": "Point",
                        "coordinates": [e.latlng.lng, e.latlng.lat]
                    },
                    properties: {
                        radius: radius
                    }
                });

            addedObstacle = true;          
        }

        // Line or Cable
        else if (type === 'line' || type === 'cable') 
        {                                             
            latlngsLine.push(e.latlng); // Add clicked point to line array   

            lineMarkers.forEach(m => map.removeLayer(m)); // Remove previous line points
            lineMarkers = [];
                    
            // Add custom circle markers for each line point
            latlngsLine.forEach(pt => {
                const circleIcon = L.divIcon({
                    className: 'circle-marker',
                    html: `
                        <div style="
                            width:20px;
                            height:20px; 
                            background-color:red;
                            border-radius:50%;
                            border: 2px solid darkred;
                            display:flex;
                            align-items:center; 
                            justify-content:center;
                            box-sizing:border-box;                            
                        "> 
                            <div style="
                                width:8px;
                                height:8px;
                                background-color:darkred;
                                border-radius:50%;
                            "></div>
                        </div>
                        `,
                    iconSize: [20, 20],
                    iconAnchor: [10, 10]
                });
                const m = L.marker(pt, { icon: circleIcon }).addTo(map);
                lineMarkers.push(m);
            });

            // Draw line if more than 1 point
            if (latlngsLine.length > 1) 
            {
                if (line) map.removeLayer(line);

                let lineOptions = { color: '#000000', weight: 3 };
                if (type === 'cable') {
                    lineOptions.dashArray = '5, 10'; // Dashed line for cable
                }

                line = L.polyline(latlngsLine, lineOptions).addTo(map);
            }

            let totalLength = 0;

            // Calculate total length of line
            for (let i = 1; i < latlngsLine.length; i++) {
                totalLength += latlngsLine[i - 1].distanceTo(latlngsLine[i]); // in meters
            }

            addedObstacle = true;

            // Update hidden fields
            document.getElementById('ObstacleGeoJson').value = JSON.stringify({
                type: "Feature",
                geometry: {
                    type: "LineString",
                    coordinates: latlngsLine.map(p => [p.lng, p.lat])
                },
                properties: {}
            });

            document.getElementById('ObstacleLineLength').value = totalLength.toFixed(2); // Update length field
            document.getElementById('ObstacleLineCoordinates').value =
                latlngsLine
                    .map(p => `Lat: ${p.lat.toFixed(6)}, Lng: ${p.lng.toFixed(6)}`)
                    .join('\n'); // Update coordinates field from line points
        }

        // Show delete button if an obstacle is added
        if (addedObstacle) showDeleteButton();
    });

    // Geolocation: show helicopter marker
    if ('geolocation' in navigator) {
        navigator.geolocation.watchPosition(function (pos) {
            var lat = pos.coords.latitude;
            var lng = pos.coords.longitude;

            map.setView([lat, lng], 14); // Center map to user location

            if (!helicopterMarker) {
                helicopterMarker = L.marker([lat, lng], { icon: helicopterIcon }).addTo(map);
            } else {
                helicopterMarker.setLatLng([lat, lng]);
            }
        });
    }

    // Update map size after load
    window.addEventListener('resize', function () { map.invalidateSize(); });
});