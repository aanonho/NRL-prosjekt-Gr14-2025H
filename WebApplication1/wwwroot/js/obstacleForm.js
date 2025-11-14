//obstacleForm.js

// -- DOM Elements -- //
// Show/hide relevant input fields based on selected obstacle type
const obstacleTypeHidden = document.getElementById('ObstacleTypeHidden')

const areaRadiusContainer = document.getElementById('areaRadiusContainer');
const lineLengthContainer = document.getElementById('lineLengthContainer');
const lineCoordinatesContainer = document.getElementById('lineCoordinatesContainer');

// Handle image upload: preview selected image and allow removing it
const fileInput = document.getElementById('ObstacleImage');
const imagePreview = document.getElementById('imagePreview');
const imageContainer = document.getElementById('imageContainer');
const removeButton = document.getElementById('removeImageButton');

const deleteButton = document.getElementById('deleteObstacleButton');
const modal = document.getElementById('obstacleFormModal');
const closeModalButton = document.getElementById('closeModal');

const obstacleButtons = document.querySelectorAll('.obstacle-button');


// -- Variables -- //
var map;
var helicopterMarker;
var obstacleMarker;
var circle;
var line;

var latlngsLine = [];
var lineMarkers = [];

var currentUserLat = null;
var currentUserLng = null;

// Helicopter icon
var helicopterIcon = L.icon({
    iconUrl: '/icons/helicopter.svg',
    iconSize: [60, 60],
    iconAnchor: [30, 30]
});

// -- Event Listeners and Functions -- //

// Show image preview when user selects a file
function setupImageUpload() {
    fileInput.addEventListener('change', (event) => {
        const file = event.target.files[0];
        if (!file) return;


        // Read the file and set it as the src of the image preview
        const reader = new FileReader();
        reader.onload = () => {
            imagePreview.src = reader.result;
            imageContainer.classList.remove('hidden');
        };
        reader.readAsDataURL(file);
    });

    // Remove selected image button logic
    removeButton.addEventListener('click', function () {
        fileInput.value = ""; // removes file input
        imagePreview.src = "";
        imageContainer.classList.add('hidden');
    });
}

// Set submit type (Submit or SaveDraft)
window.setSubmitType = function (value) {
    document.getElementById('submitType').value = value;
}

// -- Clear Map Obstacle Function -- //
function clearMapObstacle() {
    // Remove all drawn layers from map
    if (obstacleMarker) { map.removeLayer(obstacleMarker); obstacleMarker = null; }
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

// Update input field visibility based on selected obstacle type
function updatedFieldVisibility(type) {
    
    areaRadiusContainer.style.display = (type === 'area') ? 'block' : 'none';

    const showLineInputs = (type === 'line');
    lineLengthContainer.style.display = showLineInputs ? 'block' : 'none';
    lineCoordinatesContainer.style.display = showLineInputs ? 'block' : 'none';

    document.getElementById('latContainer').style.display = 'none';
    document.getElementById('lngContainer').style.display = 'none';

    //const showLatLng = (type === 'point' || type === 'area');
    //document.getElementById('latContainer').style.display = showLatLng ? 'block' : 'none';
    //document.getElementById('lngContainer').style.display = showLatLng ? 'block' : 'none';

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

// Show delete button
function showDeleteButton() {
    deleteButton.style.display = 'inline-block';
}

// Initialize obstacle type buttons
function initButtons() {
    obstacleButtons.forEach(button => {
        button.addEventListener('click', function () {
            const type = button.dataset.type;

            obstacleTypeHidden.value = type;

            updatedFieldVisibility(type);

            obstacleButtons.forEach(btn => btn.classList.remove('active'));
            button.classList.add('active');

            // Clear map is type changes
            clearMapObstacle();
        });
    });

    if (closeModalButton) {
        closeModalButton.addEventListener('click', () => {
            if (modal) modal.classList.add('hidden');
        });
    }
}

// Update hidden fields for point and area obstacles
function updateObstacleFields(latlng, radius = null) {
    const geoJson = {
        type: "Feature",
        geometry: {
            type: "Point",
            coordinates: [latlng.lng, latlng.lat]
        },
        properties: {}
    };

    if (radius !== null) geoJson.properties.radius = radius;

    document.getElementById('ObstacleLatitude').value = latlng.lat.toFixed(6);
    document.getElementById('ObstacleLongitude').value = latlng.lng.toFixed(6);
    document.getElementById('ObstacleGeoJson').value = JSON.stringify(geoJson);
}

// Draw line obstacle and update related fields
function drawLine(latlngs) {
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
    if (latlngsLine.length > 1) {
        if (line) map.removeLayer(line);
     
        line = L.polyline(latlngsLine, { color: '#000000', weight: 3 }).addTo(map);
    }

    let totalLength = 0;

    // Calculate total length of line
    for (let i = 1; i < latlngsLine.length; i++) {
        totalLength += latlngsLine[i - 1].distanceTo(latlngsLine[i]); // in meters
    }


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

    return totalLength;
}


// -- Initialize Map and Handlers -- //
document.addEventListener('DOMContentLoaded', function () {
    map = L.map('map').setView([58.1467, 7.9956], 12);

    // Add OpenStreetMap tile layer
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);

    setupImageUpload();
    initButtons();

    const defaultButton = document.querySelector('.obstacle-button[data-type="point"]');
    if (defaultButton) {
        defaultButton.click();
    }

    // Delete obstacle button logic
    deleteButton.style.display = 'none';
    deleteButton.addEventListener('click', clearMapObstacle);


    // Handle map clicks to draw obstacls
    map.on('click', function (e) {
        const type = obstacleTypeHidden.value;
        let addedObstacle = false; // True if an obstacle exists and can be deleted

        if (modal && type) modal.classList.remove('hidden');
        
        // Point
        if (type === 'point') // For point, update fields directly
        {
            clearMapObstacle(); // Reset map and input fields
            updateObstacleFields(e.latlng);
            obstacleMarker = L.marker(e.latlng).addTo(map);
            addedObstacle = true;

            document.getElementById('latContainer').style.display = 'block';
            document.getElementById('lngContainer').style.display = 'block';


        // Area
        } else if (type === 'area') {
            clearMapObstacle(); // Reset map and input fields 
            const radiusInput = document.getElementById('ObstacleRadius');
            const radius = parseFloat(radiusInput.value) || 300; // Default radius

            circle = L.circle(e.latlng, { color: 'red', fillColor: '#f03', fillOpacity: 0.5, radius: radius }).addTo(map); // Add circle with radius          
            updateObstacleFields(e.latlng, radius);         
            addedObstacle = true;

            document.getElementById('latContainer').style.display = 'block';
            document.getElementById('lngContainer').style.display = 'block';

        // Line
        } else if (type === 'line') {
            latlngsLine.push(e.latlng); // Add clicked point to line array   
            drawLine(latlngsLine);
            addedObstacle = true;
        }


        // Show delete button if an obstacle is added
        if (addedObstacle) showDeleteButton();
    });

    // -- Geolocation Handling -- //
    if ('geolocation' in navigator) {
        navigator.geolocation.watchPosition(function (pos) {
            var lat = pos.coords.latitude;
            var lng = pos.coords.longitude;

            currentUserLat = lat; // Saves GPS position
            currentUserLng = lng;

            map.setView([lat, lng], 14); // Center map to user location

            if (!helicopterMarker) {
                helicopterMarker = L.marker([lat, lng], { icon: helicopterIcon }).addTo(map);
            } else {
                helicopterMarker.setLatLng([lat, lng]);
            }
        });
    }

    document.querySelector('form').addEventListener('submit', function (e) {
        const type = obstacleTypeHidden.value;

        // If no obstacle type is selected, fall back to user's GPS position
        if (!obstacleTypeHidden.value && currentUserLat && currentUserLng) {          
                // Set lat and long from user's current postiton
                document.getElementById('ObstacleLatitude').value = currentUserLat.toFixed(6);
                document.getElementById('ObstacleLongitude').value = currentUserLng.toFixed(6);
                document.getElementById('ObstacleGeoJson').value = JSON.stringify({
                    type: "Feature",
                    geometry: {
                        type: "Point",
                        coordinates: [currentUserLng, currentUserLat]
                    },
                    properties: { source: "gps-fallback" }
                });
            }       
    });

    // Update map size after load
    window.addEventListener('resize', function () { map.invalidateSize(); });
});