document.addEventListener('DOMContentLoaded', function() {
    setTimeout(function() {
        window.originalLoadRecipes = window.loadRecipes;
        window.loadRecipes = function() {
            console.log('Using enhanced loadRecipes function');
            const container = document.getElementById('recipes-container');
            container.innerHTML = `
                <div class="d-flex justify-content-center py-4">
                    <div class="spinner-border" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                </div>
            `;
            
            fetch('/api/recipes')
                .then(response => response.json())
                .then(data => {

                    container.innerHTML = '';
                    
                    // Handle ASP.NET Core JSON reference format with $values
                    const recipesArray = Array.isArray(data) ? data : 
                                          (data.$values || data.value || []);

                    

            window.allRecipes = recipesArray;
            

            recipesArray.sort((a, b) => {
                const servingA = a.servingSize || 0;
                const servingB = b.servingSize || 0;
                return servingB - servingA;
            });
            
            if (recipesArray.length === 0) {
                container.innerHTML = '<div class="alert alert-info"><i class="fas fa-info-circle me-2"></i>No recipes available. Add some recipes to get started.</div>';
                return;
            }
                    
                    recipesArray.forEach(recipe => {
                        if (recipe === '$id') return;
                        
                        const card = document.createElement('div');
                        card.className = 'd-flex justify-content-between align-items-center recipe-item mb-2 p-3';
                        card.style.backgroundColor = 'rgba(255,255,255,0.05)';
                        card.style.border = '1px solid rgba(255,255,255,0.1)';
                        card.style.borderRadius = '8px';
                        card.style.transition = 'all 0.2s ease';
                        

                        const recipeIngredients = recipe.ingredients && recipe.ingredients.$values ? recipe.ingredients.$values : 
                                               (recipe.ingredients || []);
                        
                        card.innerHTML = `
                            <div class="recipe-info" style="cursor: pointer; flex-grow: 1;">
                                <div>
                                    <i class="fas fa-utensils me-2" style="color: white;"></i>
                                    <span class="recipe-name" style="color: white; font-weight: 500;">${recipe.name}</span>
                                </div>
                            </div>
                            <div>
                                <span class="badge bg-success me-2">Feeds ${recipe.servingSize}</span>
                                <button class="btn btn-sm btn-outline-warning edit-recipe-btn me-1" data-id="${recipe.id}" data-name="${recipe.name}">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button class="btn btn-sm btn-outline-danger delete-recipe-btn" data-id="${recipe.id}" data-name="${recipe.name}">
                                    <i class="fas fa-trash-alt"></i>
                                </button>
                            </div>
                        `;
                        

                        card.addEventListener('mouseover', function() {
                            this.style.backgroundColor = 'rgba(255,255,255,0.1)';
                            this.style.transform = 'translateY(-2px)';
                        });
                        
                        card.addEventListener('mouseout', function() {
                            this.style.backgroundColor = 'rgba(255,255,255,0.05)';
                            this.style.transform = 'translateY(0)';
                        });
                        

                        const recipeInfo = card.querySelector('.recipe-info');
                        recipeInfo.addEventListener('click', function(e) {
                            e.preventDefault();
                            

                            let ingredientsListHtml = '';
                            recipeIngredients.forEach(ri => {
                                const ingredientName = ri.ingredient && ri.ingredient.name ? ri.ingredient.name : 'Unknown';
                                ingredientsListHtml += `<li>${ri.requiredQuantity} x ${ingredientName}</li>`;
                            });
                            

                            document.getElementById('recipeDetailsModalLabel').textContent = recipe.name;
                            document.getElementById('recipeServingSize').textContent = recipe.servingSize;
                            document.getElementById('recipeIngredientsList').innerHTML = ingredientsListHtml;
                            

                            const recipeDetailsModal = new bootstrap.Modal(document.getElementById('recipeDetailsModal'));
                            recipeDetailsModal.show();
                        });
                        
                        container.appendChild(card);
                    });
                })
                .catch(error => {
                    console.error('Error loading recipes:', error);
                    container.innerHTML = 
                        '<div class="alert alert-danger"><i class="fas fa-exclamation-triangle me-2"></i>Error loading recipes</div>';
                });
        };
        

        window.originalLoadIngredients = window.loadIngredients;
        window.loadIngredients = function() {

            const container = document.getElementById('ingredients-container');
            container.innerHTML = `
                <div class="d-flex justify-content-center py-4">
                    <div class="spinner-border" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                </div>
            `;
            
            fetch('/api/ingredients')
                .then(response => response.json())
                .then(data => {

                    container.innerHTML = '';
                    
                    // Handle ASP.NET Core JSON reference format with $values
                    const ingredientsArray = Array.isArray(data) ? data : 
                                          (data.$values || data.value || []);

                    

            window.allIngredients = ingredientsArray;
            

            ingredientsArray.sort((a, b) => {
                const quantityA = a.availableQuantity || a.quantity || a.available || 0;
                const quantityB = b.availableQuantity || b.quantity || b.available || 0;
                return quantityB - quantityA; // Descending order
            });
            
            // Populate ingredient dropdowns in recipe forms
            if (window.populateIngredientDropdowns) {
                window.populateIngredientDropdowns();
            }
            
            if (ingredientsArray.length === 0) {
                container.innerHTML = '<div class="alert alert-info"><i class="fas fa-info-circle me-2"></i>No ingredients available. Add some ingredients to get started.</div>';
                return;
            }
            
            ingredientsArray.forEach(ingredient => {
                        // Skip $id properties
                        if (ingredient === '$id') return;
                        
                        const card = document.createElement('div');
                        card.className = 'd-flex justify-content-between align-items-center ingredient-item mb-2 p-2';
                        
                        // Log the ingredient object to see its structure
                        console.log('Processing ingredient:', ingredient);
                        
                        // More robust handling of quantity property
                        // Backend API returns 'availableQuantity' property
                        let quantity = 0;
                        if (ingredient.availableQuantity !== undefined && ingredient.availableQuantity !== null) {
                            quantity = ingredient.availableQuantity;
                        } else if (ingredient.quantity !== undefined && ingredient.quantity !== null) {
                            quantity = ingredient.quantity;
                        } else if (ingredient.available !== undefined && ingredient.available !== null) {
                            quantity = ingredient.available;
                        }
                        
                        // Ensure quantity is a number
                        quantity = parseInt(quantity) || 0;
                        
                        card.innerHTML = `
                            <div>
                                <i class="fas fa-carrot me-2" style="color: white;"></i>${ingredient.name}
                            </div>
                            <div>
                                <span class="badge bg-primary me-2">${quantity} available</span>
                                <button class="btn btn-sm btn-outline-warning edit-ingredient-btn me-1" data-id="${ingredient.id}" data-name="${ingredient.name}" data-quantity="${quantity}">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button class="btn btn-sm btn-outline-danger delete-ingredient-btn" data-id="${ingredient.id}" data-name="${ingredient.name}">
                                    <i class="fas fa-trash-alt"></i>
                                </button>
                            </div>
                        `;
                        container.appendChild(card);
                    });
                    
                    // Add event listeners for edit and delete buttons
                    addIngredientButtonListeners();
                })
                .catch(error => {
                    console.error('Error loading ingredients:', error);
                    container.innerHTML = 
                        '<div class="alert alert-danger"><i class="fas fa-exclamation-triangle me-2"></i>Error loading ingredients</div>';
                });
        };
        
        function addIngredientRowToEditForm(ingredientId, quantity) {
            const container = document.getElementById('editRecipeIngredientsContainer');
            const row = document.createElement('div');
            row.className = 'row mb-2 recipe-ingredient-row';
            
            row.innerHTML = `
                <div class="col-md-8">
                    <select class="form-select ingredient-select" required>
                        <option value="">Select Ingredient</option>

                    </select>
                </div>
                <div class="col-md-3">
                    <input type="number" class="form-control ingredient-quantity" placeholder="Quantity" min="1" value="${quantity}" required>
                </div>
                <div class="col-md-1">
                    <button type="button" class="btn btn-danger remove-ingredient"><i class="fas fa-times"></i></button>
                </div>
            `;
            
            container.appendChild(row);
            

            const select = row.querySelector('.ingredient-select');
            if (window.allIngredients) {
                window.allIngredients.forEach(ingredient => {
                    const option = document.createElement('option');
                    option.value = ingredient.id;
                    option.textContent = ingredient.name;
                    if (ingredient.id == ingredientId) {
                        option.selected = true;
                    }
                    select.appendChild(option);
                });
            }
            

            const removeBtn = row.querySelector('.remove-ingredient');
            removeBtn.addEventListener('click', function() {
                row.remove();
            });
        }
        
        function addIngredientButtonListeners() {
            console.log('Setting up ingredient button listeners with event delegation');
        }
        

        document.addEventListener('click', function(e) {

            if (e.target.closest('.edit-recipe-btn')) {
                e.preventDefault();
                const button = e.target.closest('.edit-recipe-btn');
                const recipeId = button.getAttribute('data-id');
                
                console.log('Edit recipe button clicked for recipe ID:', recipeId);
                
                // Fetch recipe details from API
                fetch(`/api/recipes/${recipeId}`)
                    .then(response => {
                        if (!response.ok) {
                            throw new Error('Failed to fetch recipe details');
                        }
                        return response.json();
                    })
                    .then(recipe => {
                        console.log('Recipe details fetched:', recipe);
                        
                        // Populate edit form
                        document.getElementById('editRecipeId').value = recipe.id;
                        document.getElementById('editRecipeName').value = recipe.name;
                        document.getElementById('editRecipeServingSize').value = recipe.servingSize;
                        
                        // Clear existing ingredients in edit form
                        const container = document.getElementById('editRecipeIngredientsContainer');
                        container.innerHTML = '';
                        
                        // Add recipe ingredients to edit form
                        const ingredients = recipe.ingredients && recipe.ingredients.$values ? recipe.ingredients.$values : (recipe.ingredients || []);
                        ingredients.forEach(ri => {
                            addIngredientRowToEditForm(ri.ingredient.id, ri.requiredQuantity);
                        });
                        
                        // Show modal
                        const editModal = new bootstrap.Modal(document.getElementById('editRecipeModal'));
                        editModal.show();
                    })
                    .catch(error => {
                        console.error('Error fetching recipe details:', error);
                        alert('Failed to load recipe details. Please try again.');
                    });
            }
            
            // Handle delete recipe button clicks
            if (e.target.closest('.delete-recipe-btn')) {
                e.preventDefault();
                const button = e.target.closest('.delete-recipe-btn');
                const recipeId = button.getAttribute('data-id');
                const recipeName = button.getAttribute('data-name');
                
                console.log('Delete recipe button clicked for recipe:', { id: recipeId, name: recipeName });
                
                // Populate delete confirmation
                document.getElementById('deleteRecipeId').value = recipeId;
                document.getElementById('deleteRecipeName').textContent = recipeName;
                
                // Show modal
                const deleteModal = new bootstrap.Modal(document.getElementById('deleteRecipeModal'));
                deleteModal.show();
            }
            
            // Handle save recipe button clicks
            if (e.target.closest('#saveRecipeBtn')) {
                e.preventDefault();
                const button = e.target.closest('#saveRecipeBtn');
                
                console.log('Save recipe button clicked via event delegation');
                
                // Disable button and show loading
                button.disabled = true;
                const originalText = button.innerHTML;
                button.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Saving...';
                
                // Try multiple ways to find the serving size element
                const recipeNameElement = document.getElementById('recipeName');
                let servingSizeElement = document.getElementById('recipeServingSize');
                
                console.log('Recipe name element:', recipeNameElement);
                console.log('Serving size element by ID:', servingSizeElement);
                
                // Check if there are multiple elements with this ID
                const allServingSizeElements = document.querySelectorAll('#recipeServingSize');
                console.log('All serving size elements found:', allServingSizeElements.length);
                allServingSizeElements.forEach((el, index) => {
                    console.log(`Serving size element ${index}:`, el, 'value:', el.value, 'type:', el.type);
                });
                
                // Try to find it within the add recipe modal specifically
                const addRecipeModal = document.getElementById('addRecipeModal');
                const servingSizeInModal = addRecipeModal ? addRecipeModal.querySelector('#recipeServingSize') : null;
                console.log('Serving size element in add modal:', servingSizeInModal);
                
                // Use the one from the modal if available
                if (servingSizeInModal) {
                    servingSizeElement = servingSizeInModal;
                }
                
                const recipeName = recipeNameElement ? recipeNameElement.value.trim() : '';
                const servingSizeRaw = servingSizeElement ? servingSizeElement.value : '';
                const servingSize = servingSizeRaw ? parseInt(servingSizeRaw) : 0;
                
                console.log('Final serving size element used:', servingSizeElement);
                console.log('Recipe name value:', recipeName);
                console.log('Serving size raw value:', servingSizeRaw);
                console.log('Serving size parsed value:', servingSize);
                console.log('Serving size element exists:', !!servingSizeElement);
                console.log('Serving size element value:', servingSizeElement ? servingSizeElement.value : 'N/A');
                console.log('Serving size element type:', servingSizeElement ? servingSizeElement.type : 'N/A');
                console.log('Serving size element tagName:', servingSizeElement ? servingSizeElement.tagName : 'N/A');
                
                // Validate required fields
                if (!recipeName || servingSize <= 0 || isNaN(servingSize) || !servingSizeRaw.trim()) {
                    console.log('Validation failed:', { recipeName, servingSize, servingSizeRaw, isNaN: isNaN(servingSize) });
                    alert(`Please fill in all required fields. Recipe name: "${recipeName}", Serving size: ${servingSize} (raw: "${servingSizeRaw}")`);
                    button.disabled = false;
                    button.innerHTML = originalText;
                    return;
                }
                
                // Collect ingredients from the form
                const ingredientRows = document.querySelectorAll('#recipeIngredientsContainer .recipe-ingredient-row');
                const ingredients = [];
                
                ingredientRows.forEach(row => {
                    const ingredientId = parseInt(row.querySelector('.ingredient-select').value);
                    const quantity = parseInt(row.querySelector('.ingredient-quantity').value);
                    
                    if (ingredientId && quantity) {
                        // Find the ingredient object to include in the DTO
                        const ingredient = window.allIngredients.find(ing => ing.id === ingredientId);
                        
                        ingredients.push({
                            id: 0, // For new recipe ingredients
                            recipeId: 0, // Will be set by backend
                            ingredientId: ingredientId,
                            requiredQuantity: quantity,
                            ingredient: {
                                id: ingredientId,
                                name: ingredient ? ingredient.name : '',
                                availableQuantity: ingredient ? ingredient.availableQuantity : 0
                            }
                        });
                    }
                });
                
                if (ingredients.length === 0) {
                    alert('Please add at least one ingredient to the recipe.');
                    button.disabled = false;
                    button.innerHTML = originalText;
                    return;
                }
                
                const recipeDto = {
                    id: 0, // New recipe
                    name: recipeName,
                    servingSize: servingSize,
                    ingredients: ingredients
                };
                
                console.log('Sending new recipe:', recipeDto);
                
                fetch('/api/recipes', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(recipeDto)
                })
                .then(response => {
                    if (!response.ok) {
                        return response.json().then(errorData => {
                            throw new Error(JSON.stringify(errorData, null, 2));
                        });
                    }
                    return response.json();
                })
                .then(data => {
                    console.log('Recipe created successfully:', data);
                    
                    // Clear the form
                    document.getElementById('addRecipeForm').reset();
                    document.getElementById('recipeIngredientsContainer').innerHTML = `
                        <div class="row mb-2 recipe-ingredient-row">
                            <div class="col-md-8">
                                <select class="form-select ingredient-select" required>
                                    <option value="">Select Ingredient</option>
                                </select>
                            </div>
                            <div class="col-md-3">
                                <input type="number" class="form-control ingredient-quantity" placeholder="Quantity" min="1" required>
                            </div>
                            <div class="col-md-1">
                                <button type="button" class="btn btn-danger remove-ingredient"><i class="fas fa-times"></i></button>
                            </div>
                        </div>
                    `;
                    
                    // Re-populate ingredient dropdown in the reset form
                    const select = document.querySelector('#recipeIngredientsContainer .ingredient-select');
                    if (window.allIngredients && select) {
                        window.allIngredients.forEach(ingredient => {
                            if (ingredient === '$id') return;
                            const option = document.createElement('option');
                            option.value = ingredient.id;
                            option.textContent = ingredient.name;
                            select.appendChild(option);
                        });
                    }
                    
                    // Close modal
                    const modal = bootstrap.Modal.getInstance(document.getElementById('addRecipeModal'));
                    if (modal) {
                        modal.hide();
                    }
                    
                    // Refresh recipes list
                    if (window.loadRecipes) {
                        window.loadRecipes();
                    }
                    
                    // Show success message
                    alert('Recipe created successfully!');
                })
                .catch(error => {
                    console.error('Error creating recipe:', error);
                    alert('Failed to create recipe: ' + error.message);
                })
                .finally(() => {
                    // Re-enable button
                    button.disabled = false;
                    button.innerHTML = originalText;
                });
            }
            

            if (e.target.closest('#updateRecipeBtn')) {
                e.preventDefault();
                const button = e.target.closest('#updateRecipeBtn');
                
                console.log('Update recipe button clicked via event delegation');
                
                // Disable button and show loading
                button.disabled = true;
                const originalText = button.innerHTML;
                button.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Updating...';
                
                const recipeId = document.getElementById('editRecipeId').value;
                const recipeName = document.getElementById('editRecipeName').value.trim();
                const servingSize = parseInt(document.getElementById('editRecipeServingSize').value);
                
                // Validate required fields
                if (!recipeName || servingSize <= 0 || isNaN(servingSize)) {
                    alert('Please fill in all required fields.');
                    button.disabled = false;
                    button.innerHTML = originalText;
                    return;
                }
                

                const ingredientRows = document.querySelectorAll('#editRecipeIngredientsContainer .recipe-ingredient-row');
                const ingredients = [];
                
                ingredientRows.forEach(row => {
                    const ingredientId = parseInt(row.querySelector('.ingredient-select').value);
                    const quantity = parseInt(row.querySelector('.ingredient-quantity').value);
                    
                    if (ingredientId && quantity) {
                        // Find the ingredient object to include in the DTO
                        const ingredient = window.allIngredients.find(ing => ing.id === ingredientId);
                        
                        ingredients.push({
                            id: 0, // Backend will handle this
                            recipeId: parseInt(recipeId),
                            ingredientId: ingredientId,
                            requiredQuantity: quantity,
                            ingredient: {
                                id: ingredientId,
                                name: ingredient ? ingredient.name : '',
                                availableQuantity: ingredient ? ingredient.availableQuantity : 0
                            }
                        });
                    }
                });
                
                if (ingredients.length === 0) {
                    alert('Please add at least one ingredient to the recipe.');
                    button.disabled = false;
                    button.innerHTML = originalText;
                    return;
                }
                
                const recipeDto = {
                    id: parseInt(recipeId),
                    name: recipeName,
                    servingSize: servingSize,
                    ingredients: ingredients
                };
                
                console.log('Sending recipe update via event delegation:', recipeDto);
                
                fetch(`/api/recipes/${recipeId}`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(recipeDto)
                })
                .then(response => {
                    if (!response.ok) {
                        return response.json().then(errorData => {
                            throw new Error(JSON.stringify(errorData, null, 2));
                        });
                    }
                    // Handle 204 No Content response
                    if (response.status === 204) {
                        return null;
                    }
                    return response.json();
                })
                .then(data => {
                    console.log('Recipe updated successfully via event delegation:', data);
                    
                    // Close modal
                    const modal = bootstrap.Modal.getInstance(document.getElementById('editRecipeModal'));
                    if (modal) {
                        modal.hide();
                    }
                    
                    // Refresh recipes list
                    if (window.loadRecipes) {
                        window.loadRecipes();
                    }
                    
                    // Show success message
                    alert('Recipe updated successfully!');
                })
                .catch(error => {
                    console.error('Error updating recipe via event delegation:', error);
                    alert('Failed to update recipe: ' + error.message);
                })
                .finally(() => {
                    // Re-enable button
                    button.disabled = false;
                    button.innerHTML = originalText;
                });
            }
            

            if (e.target.closest('.edit-ingredient-btn')) {
                e.preventDefault();
                const button = e.target.closest('.edit-ingredient-btn');
                const id = button.getAttribute('data-id');
                const name = button.getAttribute('data-name');
                const quantity = button.getAttribute('data-quantity');
                
                console.log('Edit button clicked for ingredient:', { id, name, quantity });
                
                // Populate edit form
                document.getElementById('editIngredientId').value = id;
                document.getElementById('editIngredientName').value = name;
                document.getElementById('editIngredientQuantity').value = quantity;
                
                // Show modal
                const editModal = new bootstrap.Modal(document.getElementById('editIngredientModal'));
                editModal.show();
            }
            

            if (e.target.closest('.delete-ingredient-btn')) {
                e.preventDefault();
                const button = e.target.closest('.delete-ingredient-btn');
                const id = button.getAttribute('data-id');
                const name = button.getAttribute('data-name');
                
                console.log('Delete button clicked for ingredient:', { id, name });
                
                // Populate delete confirmation
                document.getElementById('deleteIngredientId').value = id;
                document.getElementById('deleteIngredientName').textContent = name;
                
                // Show modal
                const deleteModal = new bootstrap.Modal(document.getElementById('deleteIngredientModal'));
                deleteModal.show();
            }
        });
        

        const updateBtn = document.getElementById('updateIngredientBtn');
        if (updateBtn) {
            // Remove existing event listeners by cloning the button
            const newUpdateBtn = updateBtn.cloneNode(true);
            updateBtn.parentNode.replaceChild(newUpdateBtn, updateBtn);
            
            // Add the new event listener with proper error handling
            newUpdateBtn.addEventListener('click', function(e) {
                // Prevent default behavior and multiple clicks
                e.preventDefault();
                
                // Disable button temporarily to prevent double-clicks
                const originalText = this.innerHTML;
                this.disabled = true;
                this.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Updating...';
                const id = document.getElementById('editIngredientId').value;
                const name = document.getElementById('editIngredientName').value.trim();
                const quantity = parseInt(document.getElementById('editIngredientQuantity').value);
                
                if (!id || !name || isNaN(quantity) || quantity < 1) {
                    alert('Please enter a valid ingredient name and quantity.');
                    return;
                }
                
                // Create ingredient object with correct property names for the API
                const updatedIngredient = {
                    id: parseInt(id),
                    name: name,
                    availableQuantity: quantity  // Backend expects 'availableQuantity', not 'quantity'
                };
                
                console.log('Updating ingredient with data:', updatedIngredient);
                
                // Send PUT request to API
                fetch(`/api/ingredients/${id}`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(updatedIngredient)
                })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Failed to update ingredient');
                    }
                    
                    // Handle empty responses or 204 No Content
                    if (response.status === 204) {
                        return {};
                    }
                    
                    // Try to parse JSON, but handle empty responses gracefully
                    return response.text().then(text => {
                        if (!text) return {};
                        try {
                            return JSON.parse(text);
                        } catch (e) {
                            console.log('Response was not valid JSON, continuing anyway');
                            return {};
                        }
                    });
                })
                .then(data => {
                    console.log('Update successful, response:', data);
                    
                    // Re-enable button
                    newUpdateBtn.disabled = false;
                    newUpdateBtn.innerHTML = originalText;
                    
                    // Close modal
                    const modal = bootstrap.Modal.getInstance(document.getElementById('editIngredientModal'));
                    modal.hide();
                    
                    // Force reload ingredients to refresh the UI
                    window.loadIngredients();
                })
                .catch(error => {
                    console.error('Error updating ingredient:', error);
                    
                    // Re-enable button
                    newUpdateBtn.disabled = false;
                    newUpdateBtn.innerHTML = originalText;
                    
                    alert('Failed to update ingredient. Please try again.');
                });
            });
            
            console.log('Update ingredient button handler fixed!');
        }
        
        // Also fix the add ingredient functionality
        const saveBtn = document.getElementById('saveIngredientBtn');
        if (saveBtn) {
            // Remove existing event listeners by cloning the button
            const newSaveBtn = saveBtn.cloneNode(true);
            saveBtn.parentNode.replaceChild(newSaveBtn, saveBtn);
            
            // Add the new event listener with proper error handling
            newSaveBtn.addEventListener('click', function(e) {
                // Prevent default behavior and multiple clicks
                e.preventDefault();
                
                // Disable button temporarily to prevent double-clicks
                const originalText = this.innerHTML;
                this.disabled = true;
                this.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Adding...';
                const name = document.getElementById('ingredientName').value.trim();
                const quantity = parseInt(document.getElementById('ingredientQuantity').value);
                
                if (!name || isNaN(quantity) || quantity < 1) {
                    alert('Please enter a valid ingredient name and quantity.');
                    return;
                }
                
                // Create ingredient object with correct property names for the API
                const newIngredient = {
                    name: name,
                    availableQuantity: quantity  // Backend expects 'availableQuantity', not 'quantity'
                };
                
                console.log('Adding new ingredient:', newIngredient);
                
                // Send POST request to API
                fetch('/api/ingredients', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(newIngredient)
                })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Failed to add ingredient');
                    }
                    
                    // Handle empty responses or 204 No Content
                    if (response.status === 204) {
                        return {};
                    }
                    
                    // Try to parse JSON, but handle empty responses gracefully
                    return response.text().then(text => {
                        if (!text) return {};
                        try {
                            return JSON.parse(text);
                        } catch (e) {
                            console.log('Response was not valid JSON, continuing anyway');
                            return {};
                        }
                    });
                })
                .then(data => {
                    console.log('Add successful, response:', data);
                    
                    newSaveBtn.disabled = false;
                    newSaveBtn.innerHTML = originalText;
                    const modal = bootstrap.Modal.getInstance(document.getElementById('addIngredientModal'));
                    modal.hide();
                    

                    document.getElementById('ingredientName').value = '';
                    document.getElementById('ingredientQuantity').value = '';
                    
                    // Force reload ingredients to refresh the UI
                    window.loadIngredients();
                })
                .catch(error => {
                    console.error('Error adding ingredient:', error);
                    
                    // Re-enable button
                    newSaveBtn.disabled = false;
                    newSaveBtn.innerHTML = originalText;
                    
                    alert('Failed to add ingredient. Please try again.');
                });
            });
            
            console.log('Add ingredient button handler fixed!');
        }
        

        window.loadIngredients();
        window.loadRecipes();
        
    }, 1000); // Wait 1 second to ensure all other scripts have loaded
});
