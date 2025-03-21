using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using vendoo.Models;
using static System.Net.Mime.MediaTypeNames;

namespace vendoo.Packages
{
    public class BasePackage : ExtraPackage
    {
        private readonly string _connectionString;

        public BasePackage(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected OracleConnection GetConnection()
        {
            return new OracleConnection(_connectionString);
        }


        public bool RegisterUser(UserModel model)
        {
            using (OracleConnection connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand("pkg_categories_operations.proc_add_user", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        string hashedPassword = HashPassword(model.Password);

                        command.Parameters.Add("p_name", OracleDbType.Varchar2).Value = model.Name;
                        command.Parameters.Add("p_lastname", OracleDbType.Varchar2).Value = model.LastName;
                        command.Parameters.Add("p_email", OracleDbType.Varchar2).Value = model.Email;
                        command.Parameters.Add("p_password", OracleDbType.Varchar2).Value = hashedPassword;
                        command.Parameters.Add("p_mobile", OracleDbType.Int32).Value = model.Mobile;
                        command.Parameters.Add("p_role_id", OracleDbType.Int32).Value = model.Role_id;

                        command.ExecuteNonQuery();
                        Console.WriteLine("Stored procedure executed successfully.");
                        return true;
                    }
                }
                catch (OracleException ex)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        public bool UpdateUser(UserModel model,int id)
        {
            using (OracleConnection connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand("pkg_categories_operations.proc_update_user", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        command.Parameters.Add("p_user_id", OracleDbType.Int32).Value =id;
                        command.Parameters.Add("p_name", OracleDbType.Varchar2).Value = model.Name;
                        command.Parameters.Add("p_lastname", OracleDbType.Varchar2).Value = model.LastName;
                        command.Parameters.Add("p_email", OracleDbType.Varchar2).Value = model.Email;
                        command.Parameters.Add("p_mobile", OracleDbType.Int32).Value = model.Mobile;

                        command.ExecuteNonQuery();
                        Console.WriteLine("Stored procedure executed successfully.");
                        return true;
                    }
                }
                catch (OracleException ex)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        public UserModel GetUser(int id)
        {
            UserModel user = null;
            using (OracleConnection connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand("pkg_categories_operations.proc_get_user", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        command.Parameters.Add("p_user_id", OracleDbType.Int32).Value = id;
                        OracleParameter cursorParameter = new OracleParameter
                        {
                            OracleDbType = OracleDbType.RefCursor,
                            Direction = ParameterDirection.Output
                        };                     
                        command.Parameters.Add(cursorParameter);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {                             
                                user = new UserModel
                                {
                                    Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                                    Name = reader["name"] != DBNull.Value ? reader["name"].ToString() : string.Empty,
                                    LastName = reader["lastname"] != DBNull.Value ? reader["lastname"].ToString() : string.Empty,
                                    Email = reader["email"] != DBNull.Value ? reader["email"].ToString() : string.Empty,
                                    Mobile = reader["mobile"] != DBNull.Value ? Convert.ToInt32(reader["mobile"]) : 0,
                                    Role_id = reader["role_id"] != DBNull.Value ? Convert.ToInt32(reader["role_id"]) : 0,
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching user: {ex.Message}");
                }
            }
            return user;
        }
        public bool DeleteUser(int id)
        {
            using (OracleConnection connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand("pkg_categories_operations.proc_delete_user", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        command.Parameters.Add("p_user_id", OracleDbType.Int32).Value = id;
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (OracleException ex)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public TokenModel LoginUser(LoginModel model)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    using (var command = new OracleCommand("pkg_categories_operations.proc_login_user", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_email", OracleDbType.Varchar2).Value = model.Email;
                        command.Parameters.Add("p_user_curs", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var hashedpassword = reader["password"].ToString();

                                if (ValidatePassword(model.Password, hashedpassword))
                                {
                                    return RetrieveUserInfo(connection, model.Email);
                                }
                                else
                                {
                                    return null;
                                }
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                throw;
            }
        }
        private TokenModel RetrieveUserInfo(OracleConnection connection, string username)
        {
            using (var command = new OracleCommand("pkg_categories_operations.proc_get_user_info", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("p_username", OracleDbType.Varchar2).Value = username;
                command.Parameters.Add("p_user_curs", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new TokenModel
                        {
                            Email = reader["email"].ToString(),
                            Id = Convert.ToInt32(reader["id"]),
                            Role = reader["role"].ToString()
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }


        public bool AddCategories(CategoriesModel model)
        {
            using (var connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    using (var command = new OracleCommand("pkg_categories_operations.proc_add_categories", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_category_name", OracleDbType.Varchar2).Value = model.CategoryName;
                        command.ExecuteNonQuery();

                        return true;
                    }
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"Oracle error: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General error: {ex.Message}");
                    return false;
                }
            }
        }

        public bool AddCategories1StChild(CategoryModel model)
        {
            using (var connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    using (var command = new OracleCommand("pkg_categories_operations.proc_add_1st_child", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_category_name", OracleDbType.Varchar2).Value = model.FirstChildName;
                        command.Parameters.Add("p_category_id", OracleDbType.Int32).Value = model.Category_ID;
                        command.ExecuteNonQuery();

                        return true;
                    }
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"Oracle error: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General error: {ex.Message}");
                    return false;
                }
            }
        }
        public async Task<bool> AddItemDescription(ItemDescriptionModel model, List<string> imagepath,int id)
        {
            using (var connection = GetConnection())
            {
                try
                {
                    await connection.OpenAsync();
                    using (var command = new OracleCommand("pkg_categories_operations.proc_add_product", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_product_name", OracleDbType.Varchar2).Value = model.Product_name;
                        command.Parameters.Add("p_category_id", OracleDbType.Int32).Value =id;
                        command.Parameters.Add("p_price", OracleDbType.Int32).Value = model.Price;
                        command.Parameters.Add("p_contact", OracleDbType.Int32).Value = model.Contact;
                        string imagePathsJson = JsonConvert.SerializeObject(imagepath);
                        command.Parameters.Add("p_json", OracleDbType.Clob).Value = imagePathsJson;
                        command.Parameters.Add("p_vaucher", OracleDbType.Int32).Value = model.Vaucher;
                        command.Parameters.Add("p_sale", OracleDbType.Int32).Value = model.Sale;
                        command.Parameters.Add("p_title", OracleDbType.Varchar2).Value = model.Title;
                        command.Parameters.Add("p_location", OracleDbType.Varchar2).Value = model.Location;
                        command.Parameters.Add("p_guests", OracleDbType.Varchar2).Value = model.Guests;
                        command.Parameters.Add("p_description", OracleDbType.Varchar2).Value =model.Description;
                        command.Parameters.Add("p_offerin", OracleDbType.Varchar2).Value = model.Offerin;
                        command.Parameters.Add("p_pricein", OracleDbType.Varchar2).Value = model.Pricein;
                        command.Parameters.Add("p_menu", OracleDbType.Varchar2).Value = model.Menu;
                        command.Parameters.Add("p_womenzone", OracleDbType.Varchar2).Value =model.Womenzone;
                        command.Parameters.Add("p_menzone", OracleDbType.Varchar2).Value = model.Menzone;
                        command.Parameters.Add("p_clinicconcept", OracleDbType.Varchar2).Value = model.Clinicconcept;
                        command.Parameters.Add("p_addinfo", OracleDbType.Varchar2).Value =model.Addinfo;
                        command.Parameters.Add("p_athotel", OracleDbType.Varchar2).Value = model.AtHotel;

                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"Oracle error: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General error: {ex.Message}");
                    return false;
                }
            }
        }

        public List<CategoryModel> GetCategories1stChild(int id)
        {
            List<CategoryModel> categories = new List<CategoryModel>();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_categories_operations.proc_get_1stchild_categories", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add("p_category_id", OracleDbType.Int32).Value = id;
                    command.Parameters.Add(cursorParameter);


                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var category = new CategoryModel
                            {
                                Category_ID = Convert.ToInt32(reader["cat_id"]),
                                CategoryName = reader["category_name"].ToString(),
                                FirstChildName = reader["first_child_name"].ToString(),
                                Id = Convert.ToInt32(reader["id"])
                            };
                            categories.Add(category);
                        }
                    }
                }
            }
            return categories;
        }
           
        public List<CategoriesModel> GetCategories()
        {
            List<CategoriesModel> categories = new List<CategoriesModel>();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_categories_operations.proc_get_all_categories", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(cursorParameter);


                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var category = new CategoriesModel
                            {
                                CategoryName = reader["category_name"].ToString(),
                                Id = Convert.ToInt32(reader["id"])
                            };
                            categories.Add(category);
                        }
                    }
                }
            }
            return categories;
        }
        public List<GetCategoryModel> GetFilteredItems(int id, string category_name)
        {
            List<GetCategoryModel> categories = new List<GetCategoryModel>();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_categories_operations.proc_get_filtered_categories", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add("p_category_id", OracleDbType.Int32).Value = id;
                    command.Parameters.Add("p_category_name", OracleDbType.Varchar2).Value = category_name;

                    command.Parameters.Add(cursorParameter);


                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var category = new GetCategoryModel
                            {
                                category_name = reader["category_name"] != DBNull.Value ? reader["category_name"].ToString() : string.Empty,
                                first_child_name = reader["first_child_name"] != DBNull.Value ? reader["first_child_name"].ToString() : string.Empty,
                                title = reader["title"] != DBNull.Value ? reader["title"].ToString() : string.Empty,
                                Category_id = reader["category_id"] != DBNull.Value ? Convert.ToInt32(reader["category_id"]) : 0,
                                Main_category_id = reader["main_category_id"] != DBNull.Value ? Convert.ToInt32(reader["main_category_id"]) : 0,
                                product_name = reader["product_name"] != DBNull.Value ? reader["product_name"].ToString() : string.Empty,
                                Price = reader["price"] != DBNull.Value ? Convert.ToInt32(reader["price"]) : 0,
                                Image_path = reader["image_path"] != DBNull.Value ? JsonConvert.DeserializeObject<List<string>>(reader["image_path"].ToString()) : new List<string>(),
                                location = reader["location"] != DBNull.Value ? reader["location"].ToString() : string.Empty,
                                Guests = reader["guests"] != DBNull.Value ? reader["guests"].ToString() : string.Empty,   
                                Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                                Sale = reader["sale"] != DBNull.Value ? Convert.ToInt32(reader["sale"]) : 0,
                                Current_price = reader["current_price"] != DBNull.Value ? Convert.ToInt32(reader["current_price"]) : 0
                            };
                            categories.Add(category);
                        }
                    }
                }
            }
            return categories;
        }

        public List<GetCategoryModel> GetSearchedPRoducts(string product_name)
        {
            List<GetCategoryModel> categories = new List<GetCategoryModel>();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_categories_operations.proc_get_searched_products", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add("p_product_name", OracleDbType.Varchar2).Value = product_name;

                    command.Parameters.Add(cursorParameter);


                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var category = new GetCategoryModel
                            {
                                category_name = reader["category_name"] != DBNull.Value ? reader["category_name"].ToString() : string.Empty,
                                first_child_name = reader["first_child_name"] != DBNull.Value ? reader["first_child_name"].ToString() : string.Empty,
                                title = reader["title"] != DBNull.Value ? reader["title"].ToString() : string.Empty,
                                Category_id = reader["category_id"] != DBNull.Value ? Convert.ToInt32(reader["category_id"]) : 0,
                                Main_category_id = reader["main_category_id"] != DBNull.Value ? Convert.ToInt32(reader["main_category_id"]) : 0,
                                product_name = reader["product_name"] != DBNull.Value ? reader["product_name"].ToString() : string.Empty,
                                Price = reader["price"] != DBNull.Value ? Convert.ToInt32(reader["price"]) : 0,
                                Image_path = reader["image_path"] != DBNull.Value ? JsonConvert.DeserializeObject<List<string>>(reader["image_path"].ToString()) : new List<string>(),
                                location = reader["location"] != DBNull.Value ? reader["location"].ToString() : string.Empty,
                                Guests = reader["guests"] != DBNull.Value ? reader["guests"].ToString() : string.Empty,
                                Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                                Sale = reader["sale"] != DBNull.Value ? Convert.ToInt32(reader["sale"]) : 0,
                                Current_price = reader["current_price"] != DBNull.Value ? Convert.ToInt32(reader["current_price"]) : 0
                            };
                            categories.Add(category);
                        }
                    }
                }
            }
            return categories;
        }

        public List<GetCategoryModel> GetProducts()
        {
            List<GetCategoryModel> categories = new List<GetCategoryModel>();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_categories_operations.proc_get_products", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    

                    command.Parameters.Add(cursorParameter);


                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var category = new GetCategoryModel
                            {
                                category_name = reader["category_name"] != DBNull.Value ? reader["category_name"].ToString() : string.Empty,
                                first_child_name = reader["first_child_name"] != DBNull.Value ? reader["first_child_name"].ToString() : string.Empty,
                                title = reader["title"] != DBNull.Value ? reader["title"].ToString() : string.Empty,
                                product_name = reader["product_name"] != DBNull.Value ? reader["product_name"].ToString() : string.Empty,
                                Price = reader["price"] != DBNull.Value ? Convert.ToInt32(reader["price"]) : 0,
                                Image_path = reader["image_path"] != DBNull.Value ? JsonConvert.DeserializeObject<List<string>>(reader["image_path"].ToString()) : new List<string>(),
                                location = reader["location"] != DBNull.Value ? reader["location"].ToString() : string.Empty,
                                Guests = reader["guests"] != DBNull.Value ? reader["guests"].ToString() : string.Empty,
                                Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                                Sale = reader["sale"] != DBNull.Value ? Convert.ToInt32(reader["sale"]) : 0,
                                Current_price = reader["current_price"] != DBNull.Value ? Convert.ToInt32(reader["current_price"]) : 0
                            };
                            categories.Add(category);
                        }
                    }
                }
            }
            return categories;
        }
        public GetProductModel ItemDescription(int id)
        {
            GetProductModel products = null;

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_categories_operations.proc_get_all_products", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add("p_id", OracleDbType.Int32).Value = id;
                    command.Parameters.Add(cursorParameter);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products = new GetProductModel
                            {
                                Product_name = reader["product_name"] != DBNull.Value ? reader["product_name"].ToString() : string.Empty,
                                Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                                Category_id = reader["category_id"] != DBNull.Value ? Convert.ToInt32(reader["category_id"]) : 0,
                                Price = reader["price"] != DBNull.Value ? Convert.ToInt32(reader["price"]) : 0,
                                Contact = reader["contact"] != DBNull.Value ? Convert.ToInt32(reader["contact"]) : 0,
                                Image_path = reader["image_path"] != DBNull.Value ? JsonConvert.DeserializeObject<List<string>>(reader["image_path"].ToString()) : new List<string>(),
                                Vaucher = reader["vaucher"] != DBNull.Value ? Convert.ToInt32(reader["vaucher"]) : 0,
                                Sale = reader["sale"] != DBNull.Value ? Convert.ToInt32(reader["sale"]) : 0,
                                Current_price = reader["current_price"] != DBNull.Value ? Convert.ToInt32(reader["current_price"]) : 0,
                                Title = reader["title"] != DBNull.Value ? reader["title"].ToString() : string.Empty,
                                Description = reader["description"] != DBNull.Value ? reader["description"].ToString() : string.Empty,
                                Offerin = reader["offerin"] != DBNull.Value ? reader["offerin"].ToString() : string.Empty,
                                Pricein = reader["pricein"] != DBNull.Value ? reader["pricein"].ToString() : string.Empty,
                                Menu = reader["menu"] != DBNull.Value ? reader["menu"].ToString() : string.Empty,
                                Womenzone = reader["womenzone"] != DBNull.Value ? reader["womenzone"].ToString() : string.Empty,
                                Menzone = reader["menzone"] != DBNull.Value ? reader["menzone"].ToString() : string.Empty,
                                Clinicconcept = reader["clinicconcept"] != DBNull.Value ? reader["clinicconcept"].ToString() : string.Empty,
                                Addinfo = reader["addinfo"] != DBNull.Value ? reader["addinfo"].ToString() : string.Empty,
                                Athotel = reader["athotel"] != DBNull.Value ? reader["athotel"].ToString() : string.Empty
                            };
                        }
                    }
                }
            }
            return products;
        }

        public List<LocationModel> getLocations(int? id,int maincat_id)
        {
            List<LocationModel> locations = new List<LocationModel>();


            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_categories_operations.proc_get_locations", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add("p_maincat_id", OracleDbType.Int32).Value = maincat_id;
                    command.Parameters.Add("p_category_id", OracleDbType.Int32).Value = id;

                    command.Parameters.Add(cursorParameter);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var location = new LocationModel
                            {
                                Location = reader["location"] != DBNull.Value ? reader["location"].ToString() : string.Empty,
                                Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                            };
                            locations.Add(location);
                        }
                    }
                }
            }
            return locations;
        }

        public List<GuestNumberModel> getGuestNumber(int category_id)
        {
            List<GuestNumberModel> locations = new List<GuestNumberModel>();


            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_categories_operations.proc_get_guestnumber", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(cursorParameter);
                    command.Parameters.Add("p_category_id", OracleDbType.Int32).Value = category_id;


                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var location = new GuestNumberModel
                            {
                                Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                                GuestNumber = reader["guest_number"] != DBNull.Value ? reader["guest_number"].ToString() : string.Empty,
                            };
                            locations.Add(location);
                        }
                    }
                }
            }
            return locations;
        }

        public List<OrderModel> GetOrder()
        {
            List<OrderModel> orders = new List<OrderModel>();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_tasks_orders.proc_get_order", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(cursorParameter);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var productIdJson = reader["product_id"] != DBNull.Value ? reader["product_id"].ToString() : "[]";
                            var productIds = JsonConvert.DeserializeObject<List<int>>(productIdJson);

                            var order = new OrderModel
                            {
                                id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                                name = reader["name"] != DBNull.Value ? reader["name"].ToString() : string.Empty,
                                quantity = reader["quantity"] != DBNull.Value ? Convert.ToInt32(reader["quantity"]) : 0,
                                product_id = productIds 
                            };
                            orders.Add(order);
                        }
                    }
                }
            }
            return orders;
        }


        public async Task<bool> AddToBasket(CartModel model)
        {
            using (var connection = GetConnection())
            {
                try
                {
                    await connection.OpenAsync();
                    using (var command = new OracleCommand("pkg_categories_operations.proc_add_to_basket", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_product_id", OracleDbType.Int32).Value = model.Product_id;
                        command.Parameters.Add("p_user_id", OracleDbType.Int32).Value = model.User_id;
                        command.Parameters.Add("p_quantity", OracleDbType.Int32).Value = model.Quantity;
                        
                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"Oracle error: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General error: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<bool>UpdateBasket(CartModel model)
        {
            using (var connection = GetConnection())
            {
                try
                {
                    await connection.OpenAsync();
                    using (var command = new OracleCommand("pkg_categories_operations.proc_update_basket", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_user_id", OracleDbType.Int32).Value = model.User_id;
                        command.Parameters.Add("p_product_id", OracleDbType.Int32).Value = model.Product_id;
                        command.Parameters.Add("p_quantity", OracleDbType.Int32).Value = model.Quantity;

                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"Oracle error: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General error: {ex.Message}");
                    return false;
                }
            }
        }

        public List<GetBasketModel> GetBasket(int id)
        {
            List<GetBasketModel> basket = new List<GetBasketModel>();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_categories_operations.proc_get_basket", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("p_user_id", OracleDbType.Int32).Value = id;
                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(cursorParameter);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var bask= new GetBasketModel
                            {
                                Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                                Product_id = reader["product_id"] != DBNull.Value ? Convert.ToInt32(reader["product_id"]) : 0,
                                Image_path = reader["image_path"] != DBNull.Value ? JsonConvert.DeserializeObject<List<string>>(reader["image_path"].ToString()) : new List<string>(),
                                Title = reader["title"] != DBNull.Value ? reader["title"].ToString() : string.Empty,
                                Product_name = reader["product_name"] != DBNull.Value ? reader["product_name"].ToString() : string.Empty,
                                Vaucher_price = reader["vaucher_price"] != DBNull.Value ? Convert.ToInt32(reader["vaucher_price"]) : 0,
                                Total_quantity = reader["total_quantity"] != DBNull.Value ? Convert.ToInt32(reader["total_quantity"]) : 0,
                                Vaucher_quantity = reader["vaucher_quantity"] != DBNull.Value ? Convert.ToInt32(reader["vaucher_quantity"]) : 0,
                                Total_price = reader["total_price"] != DBNull.Value ? Convert.ToInt32(reader["total_price"]) : 0,
                                Current_price = reader["current_price"] != DBNull.Value ? Convert.ToInt32(reader["current_price"]) : 0,
                                Starting_price = reader["starting_price"] != DBNull.Value ? Convert.ToInt32(reader["starting_price"]) : 0,
                            };
                            basket.Add(bask);
                        }
                    }
                }
            }
            return basket;
        }
        public async Task<bool> DeleteFromBasket(int user_id,int product_id)
        {
            using (var connection = GetConnection())
            {
                try
                {
                    await connection.OpenAsync();
                    using (var command = new OracleCommand("pkg_categories_operations.proc_delete_from_cart", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_user_id", OracleDbType.Int32).Value = user_id;
                        command.Parameters.Add("p_product_id", OracleDbType.Int32).Value = product_id;

                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"Oracle error: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General error: {ex.Message}");
                    return false;
                }
            }
        }
    }
    }

