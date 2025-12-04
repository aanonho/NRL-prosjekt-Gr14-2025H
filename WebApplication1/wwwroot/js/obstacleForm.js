//obstacleForm.js

// -- DOM Elements -- //
// Show/hide relevant input fields based on selected obstacle type
const obstacleTypeHidden = document.getElementById('ObstacleTypeHidden')

const areaRadiusContainer = document.getElementById('areaRadiusContainer');
const lineLengthContainer = document.getElementById('lineLengthContainer');
const lineCoordinatesContainer = document.getElementById('lineCoordinatesContainer');

// Handle image upload: preview selected images and allow removing them
const fileInput = document.getElementById('ObstacleImage');
const imageContainer = document.getElementById('imageContainer');
const imagePreviewList = document.getElementById('imagePreviewList');
const removeButton = document.getElementById('removeImageButton');
const imageError = document.getElementById('imageError');
const imagesToRemoveInput = document.getElementById('ImagesToRemove');
const existingImageTiles = Array.from(document.querySelectorAll('.existing-image-tile'));

const allowedImageExtensions = ['.jpg', '.jpeg', '.png'];
const allowedImageTypes = ['image/jpeg', 'image/png'];
const maxImageSizeBytes = 10 * 1024 * 1024; // Keep in sync with server limit

const selectedFiles = [];
const selectedImageIndexes = new Set();
const existingSelectedImages = new Set();
const pendingRemovalImages = new Set();

const deleteButton = document.getElementById('deleteObstacleButton');
const modal = document.getElementById('obstacleFormModal');
const closeModalButton = document.getElementById('closeModal');

const obstacleButtons = document.querySelectorAll('.obstacle-button');
const locationButton = document.getElementById('locateUserBtn');

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
    if (!fileInput || !imageContainer || !imagePreviewList) return;

    clearImageError();
    updateRemoveButtonState();

    hydrateImagesToRemove();
    renderExistingImages();

    fileInput.addEventListener('change', (event) => {
        const files = Array.from(event.target.files || []);
        selectedImageIndexes.clear();
        clearImageError();

        if (files.length === 0 && selectedFiles.length === 0) {
            imageContainer.classList.add('hidden');
            return;
        }

        const hasInvalidFile = files.some(file => !validateFile(file));

        if (hasInvalidFile) {
            fileInput.value = '';
            renderImagePreview();
            return;
        }

        files.forEach(file => {
            const duplicate = selectedFiles.some(existing => existing.name === file.name && existing.size === file.size && existing.lastModified === file.lastModified);
            if (!duplicate) {
                selectedFiles.push(file);
            }
        });

        renderImagePreview();
        syncFileInput();
    });

    // Remove selected images button logic
    if (removeButton) {
        removeButton.addEventListener('click', function () {
            const hasNewSelection = selectedImageIndexes.size > 0;
            const hasExistingSelection = existingSelectedImages.size > 0;

            if (!hasNewSelection && !hasExistingSelection) return;

            if (hasNewSelection) {
                const remainingFiles = selectedFiles.filter((_, index) => !selectedImageIndexes.has(index));
                selectedFiles.length = 0;
                selectedFiles.push(...remainingFiles);

                selectedImageIndexes.clear();
                renderImagePreview();
                syncFileInput();
                clearImageError();
            }

            if (hasExistingSelection) {
                togglePendingRemovalForSelection();
            }

            updateRemoveButtonState();
        });
    }

    function validateFile(file) {
        const extension = (file.name || '').toLowerCase();
        const fileType = (file.type || '').toLowerCase();

        const isAllowedExtension = allowedImageExtensions.some(ext => extension.endsWith(ext));
        const isAllowedType = allowedImageTypes.includes(fileType);

        if (!isAllowedExtension || !isAllowedType) {
            setImageError('Only PNG and JPEG images are allowed (no audio or video files).');
            return false;
        }

        if (file.size > maxImageSizeBytes) {
            setImageError('Each image must be 10 MB or smaller.');
            return false;
        }

        return true;
    }

    function setImageError(message) {
        if (!imageError) return;

        const hasMessage = Boolean(message);
        imageError.textContent = message || '';
        imageError.style.display = hasMessage ? 'inline-block' : 'none';
    }

    function clearImageError() {
        setImageError('');
    }

    function updateRemoveButtonState() {
        if (!removeButton) return;

        const hasSelection = selectedImageIndexes.size > 0 || existingSelectedImages.size > 0;
        removeButton.disabled = !hasSelection;
        removeButton.classList.toggle('opacity-60', !hasSelection);
        removeButton.classList.toggle('cursor-not-allowed', !hasSelection);
        removeButton.textContent = hasSelection
            ? `Remove Selected (${selectedImageIndexes.size + existingSelectedImages.size})`
            : 'Remove Selected';
    }

    function hydrateImagesToRemove() {
        const initialRemovals = (imagesToRemoveInput?.value || '')
            .split(',')
            .map(p => p.trim())
            .filter(Boolean);

        initialRemovals.forEach(path => pendingRemovalImages.add(path));
    }

    function renderExistingImages() {
        if (!existingImageTiles.length) {
            return;
        }

        existingImageTiles.forEach(tile => {
            const path = tile.dataset.imagePath;
            const isSelected = existingSelectedImages.has(path);
            const isPendingRemoval = pendingRemovalImages.has(path);

            tile.classList.toggle('selected', isSelected);
            tile.classList.toggle('pending-removal', isPendingRemoval);
        });

        updateRemoveButtonState();
        syncImagesToRemove();
    }

    function togglePendingRemovalForSelection() {
        existingSelectedImages.forEach(path => {
            if (pendingRemovalImages.has(path)) {
                pendingRemovalImages.delete(path);
            } else {
                pendingRemovalImages.add(path);
            }
        });

        existingSelectedImages.clear();
        renderExistingImages();
    }

    function syncImagesToRemove() {
        if (!imagesToRemoveInput) return;

        imagesToRemoveInput.value = Array.from(pendingRemovalImages).join(',');
    }

    existingImageTiles.forEach(tile => {
        tile.addEventListener('click', () => {
            const path = tile.dataset.imagePath;

            if (!path) return;

            if (existingSelectedImages.has(path)) {
                existingSelectedImages.delete(path);
            } else {
                existingSelectedImages.add(path);
            }

            renderExistingImages();
        });
    });

    function renderImagePreview() {
        imagePreviewList.innerHTML = '';

        if (selectedFiles.length === 0) {
            imageContainer.classList.add('hidden');
            updateRemoveButtonState();
            return;
        }

        selectedFiles.forEach((file, index) => {
            const listItem = document.createElement('button');
            listItem.type = 'button';
            listItem.dataset.index = index;
            listItem.className = 'image-preview-tile flex w-32 flex-col gap-1 rounded border border-gray-300 p-2 text-left focus:outline-none focus:ring-2 focus:ring-blue-500';

            const isSelected = selectedImageIndexes.has(index);
            listItem.setAttribute('aria-pressed', isSelected ? 'true' : 'false');

            if (isSelected) {
                listItem.classList.add('selected');
            }

            listItem.addEventListener('click', () => {
                if (selectedImageIndexes.has(index)) {
                    selectedImageIndexes.delete(index);
                } else {
                    selectedImageIndexes.add(index);
                }

                renderImagePreview();
            });

            const selectionBadge = document.createElement('span');
            selectionBadge.className = 'selection-badge';
            selectionBadge.textContent = 'Selected';
            listItem.appendChild(selectionBadge);

            const label = document.createElement('span');
            label.className = 'truncate text-xs font-medium text-gray-800';
            label.textContent = file.name;
            listItem.appendChild(label);

            const img = document.createElement('img');
            const imageUrl = URL.createObjectURL(file);
            img.src = imageUrl;
            img.onload = () => URL.revokeObjectURL(imageUrl);
            img.alt = file.name;
            img.className = 'h-24 w-full rounded object-cover';
            listItem.appendChild(img);

            imagePreviewList.appendChild(listItem);
        });

        imageContainer.classList.remove('hidden');
        updateRemoveButtonState();
    }

    function syncFileInput() {
        if (!fileInput) return;

        const dataTransfer = new DataTransfer();
        selectedFiles.forEach(file => dataTransfer.items.add(file));
        fileInput.files = dataTransfer.files;

        if (selectedFiles.length === 0) {
            fileInput.value = '';
            imageContainer.classList.add('hidden');
        }
    }

    updateRemoveButtonState();

    // Ensure the current selection is attached to the form on submit
    const obstacleForm = document.getElementById('obstacleForm');
    if (obstacleForm) {
        obstacleForm.addEventListener('submit', () => {
            syncFileInput();
            syncImagesToRemove();
        });
    }
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

function setActiveObstacleType(type, shouldClearMap = true) {
    if (!type) return;

    obstacleTypeHidden.value = type;
    updatedFieldVisibility(type);

    obstacleButtons.forEach(btn => {
        btn.classList.toggle('active', btn.dataset.type === type);
    });

    if (shouldClearMap) {
        clearMapObstacle();
    }
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
    if (disableEditing) {
        deleteButton.style.display = 'none';
    }
    else {
        deleteButton.style.display = 'inline-block';
    }
}

// Initialize obstacle type buttons
function initButtons() {
    obstacleButtons.forEach(button => {
        button.addEventListener('click', function () {
            const type = button.dataset.type;
            setActiveObstacleType(type);
        });
    });

    if (closeModalButton) {
        closeModalButton.addEventListener('click', () => {
            if (modal) modal.classList.add('hidden');
        });
    }
}

function hydrateExistingObstacle() {
    if (!obstacleTypeHidden || !obstacleTypeHidden.value) {
        return;
    }

    const type = obstacleTypeHidden.value;
    setActiveObstacleType(type, false);

    let latValue = parseFloat(document.getElementById('ObstacleLatitude').value);
    let lngValue = parseFloat(document.getElementById('ObstacleLongitude').value);

    if ((Number.isNaN(latValue) || Number.isNaN(lngValue)) && document.getElementById('ObstacleGeoJson')?.value) {
        try {
            const parsedGeo = JSON.parse(document.getElementById('ObstacleGeoJson').value);
            if (parsedGeo?.geometry?.coordinates?.length >= 2) {
                lngValue = parseFloat(parsedGeo.geometry.coordinates[0]);
                latValue = parseFloat(parsedGeo.geometry.coordinates[1]);

                if (!Number.isNaN(latValue) && !Number.isNaN(lngValue)) {
                    document.getElementById('ObstacleLatitude').value = latValue.toFixed(6);
                    document.getElementById('ObstacleLongitude').value = lngValue.toFixed(6);
                }

                if (type === 'area' && typeof parsedGeo?.properties?.radius === 'number') {
                    document.getElementById('ObstacleRadius').value = parsedGeo.properties.radius;
                }
            }
        } catch (err) {
            console.warn('Unable to parse stored GeoJSON for obstacle', err);
        }
    }

    if ((type === 'point' || type === 'area') && !Number.isNaN(latValue) && !Number.isNaN(lngValue)) {
        const latlng = L.latLng(latValue, lngValue);
        if (type === 'area') {
            const radius = parseFloat(document.getElementById('ObstacleRadius').value) || 300;
            circle = L.circle(latlng, { color: 'red', fillColor: '#f03', fillOpacity: 0.5, radius: radius }).addTo(map);
            updateObstacleFields(latlng, radius);
        } else {
            obstacleMarker = L.marker(latlng).addTo(map);
            updateObstacleFields(latlng);
        }

        map.setView(latlng, 14);
        document.getElementById('latContainer').style.display = 'block';
        document.getElementById('lngContainer').style.display = 'block';
        showDeleteButton();
        return;
    }

    if (type === 'line') {
        const geoJsonRaw = document.getElementById('ObstacleGeoJson').value;
        if (!geoJsonRaw) return;

        try {
            const parsed = JSON.parse(geoJsonRaw);
            const coords = parsed?.geometry?.coordinates;

            if (Array.isArray(coords)) {
                latlngsLine = coords.map(pt => L.latLng(pt[1], pt[0]));
                drawLine(latlngsLine);
                if (latlngsLine.length > 0) {
                    map.fitBounds(L.latLngBounds(latlngsLine));
                    showDeleteButton();
                }
            }
        } catch (e) {
            console.warn('Unable to parse stored obstacle geometry', e);
        }
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
    //makes map globally available
    window.leafletMap = map;

    // Add OpenStreetMap tile layer
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);

    setupImageUpload();
    initButtons();

    // Delete obstacle button logic
    deleteButton.style.display = 'none';
    deleteButton.addEventListener('click', clearMapObstacle);

    const defaultButton = document.querySelector('.obstacle-button[data-type="point"]');
    if (!obstacleTypeHidden.value || obstacleTypeHidden.value.trim() === "") {
        obstacleTypeHidden.value = "point";
        if (defaultButton) defaultButton.click();
    } else {
        hydrateExistingObstacle();
    }


    // Handle map clicks to draw obstacls
    map.on('click', function (e) {
        if (disableEditing) return; // Editing disabled
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

    const obstacleForm = document.getElementById('obstacleForm');

    obstacleForm?.addEventListener('submit', function () {
        const geoJsonInput = document.getElementById('ObstacleGeoJson');
        const latInput = document.getElementById('ObstacleLatitude');
        const lngInput = document.getElementById('ObstacleLongitude');

        const latValue = parseFloat(latInput.value);
        const lngValue = parseFloat(lngInput.value);
        const hasLatLng = !Number.isNaN(latValue) && !Number.isNaN(lngValue);
        const hasGeoJson = !!geoJsonInput.value?.trim();

        // If no geometry is present but GPS is available, fall back to user's position
        const fallbackLat = currentUserLat ?? helicopterMarker?.getLatLng()?.lat ?? null;
        const fallbackLng = currentUserLng ?? helicopterMarker?.getLatLng()?.lng ?? null;

        if (!hasLatLng && !hasGeoJson && fallbackLat !== null && fallbackLng !== null) {
            latInput.value = fallbackLat.toFixed(6);
            lngInput.value = fallbackLng.toFixed(6);
            geoJsonInput.value = JSON.stringify({
                type: "Feature",
                geometry: {
                    type: "Point",
                    coordinates: [fallbackLng, fallbackLat]
                },
                properties: { source: "gps-fallback" }
            });
        }
    });

    // Locate user button logic
    locationButton.addEventListener('click', function () {
        if (currentUserLat != null && currentUserLng != null) {
            map.setView([currentUserLat, currentUserLng], 14);
        }
    });

    // Update map size after load
    window.addEventListener('resize', function () { map.invalidateSize(); });
});